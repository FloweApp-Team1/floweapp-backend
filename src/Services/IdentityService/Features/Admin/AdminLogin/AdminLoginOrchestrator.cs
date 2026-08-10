using IdentityService.Common;
using Shared.Contracts;
using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using Shared.Results;
using Shared.Security;
using Shared.Settings;
using IdentityService.Features.Admin.AdminLogin.Commands;
using IdentityService.Features.Admin.AdminLogin.Dtos;
using IdentityService.Features.Admin.AdminLogin.Queries;
using MediatR;
using Microsoft.Extensions.Options;

namespace IdentityService.Features.Admin.AdminLogin
{
    public sealed record AdminLoginCommand(
          string Email,
          string Password,
          string IpAddress,
          string UserAgent) : IRequest<Result<LoginAdminResponseDto>>;

    public sealed class AdminLoginHandler(
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions,
        ISender sender)
        : IRequestHandler<AdminLoginCommand, Result<LoginAdminResponseDto>>
    {
        // Verified against on unknown accounts so a missing user costs the same
        // wall-clock time as a wrong password.
        private const string DummyHash = "$2a$12$DummyHashStringForTimingAttackMitigationXXXXXXXXXXXX";

        private readonly TimeSpan _accessTokenLifetime =
            TimeSpan.FromMinutes(jwtOptions.Value.AccessTokenExpiryMinutes);

        public async Task<Result<LoginAdminResponseDto>> Handle(
            AdminLoginCommand request, CancellationToken ct)
        {
            var rateLimitResult = await sender.Send(new CheckAdminRateLimitQuery(request.Email, request.IpAddress), ct);
            if (rateLimitResult.Value)
            {
                await LogFailedAttempt(request, ct);
                return Result.Failure<LoginAdminResponseDto>(AuthErrors.TooManyAttempts);
            }

            var userResult = await sender.Send(new GetAdminByEmailQuery(request.Email), ct);
            var user = userResult.Value;
            var roleNames = user?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? [];
            var isAdmin = roleNames.Contains(AppRoles.Admin, StringComparer.OrdinalIgnoreCase);

            if (user is null || !isAdmin || !user.IsActive)
            {
                passwordHasher.Verify(request.Password, DummyHash);
                await LogFailedAttempt(request, ct);
                return Result.Failure<LoginAdminResponseDto>(AuthErrors.InvalidCredentials);
            }

            if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                await LogFailedAttempt(request, ct);
                return Result.Failure<LoginAdminResponseDto>(AuthErrors.InvalidCredentials);
            }

            await unitOfWork.BeginTransactionAsync(ct);
            Result<string> refreshTokenResult;
            try
            {
                refreshTokenResult = await sender.Send(new IssueRefreshTokenCommand(user.Id), ct);
                await sender.Send(new RecordAdminLoginAuditCommand(request.Email, true, request.IpAddress, request.UserAgent), ct);
                await unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            var accessToken = jwtService.GenerateAccessToken(user, roleNames);

            var userProfile = new UserProfileDto(
                user.Id,
                user.FullName,
                user.Email,
                roleNames
            );

            var responseDto = new LoginAdminResponseDto(
                userProfile,
                accessToken,
                refreshTokenResult.Value,
                DateTime.UtcNow.Add(_accessTokenLifetime)
            );

            return Result.Success(responseDto);
        }

        private async Task LogFailedAttempt(AdminLoginCommand request, CancellationToken ct)
            => await sender.Send(new RecordAdminLoginAuditCommand(request.Email, false, request.IpAddress, request.UserAgent), ct);
    }
}
