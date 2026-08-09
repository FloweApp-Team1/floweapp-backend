using IdentityService.Common;
using IdentityService.Common.Results;
using IdentityService.Common.Security;
using IdentityService.Domain.Interfaces;
using IdentityService.Features.Admin.AdminLogin.Commands;
using IdentityService.Features.Admin.AdminLogin.Dtos;
using IdentityService.Features.Admin.AdminLogin.Queries;
using MediatR;

namespace IdentityService.Features.Admin.AdminLogin
{

    public sealed record AdminLoginCommand(
          string Email,
          string Password,
          string IpAddress,
          string UserAgent) : IRequest<Result<LoginAdminResponseDto>>;


   
    public sealed class AdminLoginHandler(
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork,
        ISender sender)
        : IRequestHandler<AdminLoginCommand, Result<LoginAdminResponseDto>>
    {
        private const string DummyHash = "$2a$12$DummyHashStringForTimingAttackMitigationXXXXXXXXXXXX";
        private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);

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
            var isAdmin = user?.UserRoles.Any(ur => ur.Role.Name == RoleConstants.Admin) ?? false;

            if (user is null || !isAdmin || !user.IsActive)
            {
                passwordHasher.Verify(request.Password, DummyHash);
                await LogFailedAttempt(request, ct);
                return Result.Failure<LoginAdminResponseDto>(AuthErrors.InvalidCredentials);
            }

            var passwordValid = passwordHasher.Verify(request.Password, user.PasswordHash);
            if (!passwordValid)
            {
                await LogFailedAttempt(request, ct);
                return Result.Failure<LoginAdminResponseDto>(AuthErrors.InvalidCredentials);
            }

            await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var refreshTokenResult = await sender.Send(new IssueRefreshTokenCommand(user.Id), ct);
                await sender.Send(new RecordAdminLoginAuditCommand(request.Email, true, request.IpAddress, request.UserAgent), ct);

                

                var accessToken = tokenService.GenerateAccessToken(user, AccessTokenLifetime);

                var userProfile = new UserProfileDto(
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.UserRoles.Select(ur => ur.Role.Name).ToList()
                );

                var responseDto = new LoginAdminResponseDto(
                    userProfile,
                    accessToken,
                    refreshTokenResult.Value,
                    DateTime.UtcNow.Add(AccessTokenLifetime)
                );
                await transaction.CommitAsync(ct);
                return Result.Success(responseDto);  
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }


        }
        private async Task LogFailedAttempt(AdminLoginCommand request, CancellationToken ct)
    => await sender.Send(new RecordAdminLoginAuditCommand(request.Email, false, request.IpAddress, request.UserAgent), ct);
    }
}