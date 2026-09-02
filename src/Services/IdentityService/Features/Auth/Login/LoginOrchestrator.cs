using Shared.Interfaces;
using Shared.Results;
using IdentityService.Common.Interfaces;
using IdentityService.Features.Auth.Login.Commands;
using IdentityService.Features.Auth.Login.Queries;
using MediatR;
using IdentityService.Domain.Entities;
using Shared.Contracts;

namespace IdentityService.Features.Auth.Login
{
    public sealed class LoginOrchestrator(
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ISender sender)
        : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {

        private const string DummyHash = "$2a$12$DummyHashStringForTimingAttackMitigationXXXXXXXXXXXX";

        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken ct)
        {
            var email = request.Request.Email;

            var userResult = await sender.Send(new GetUserByEmailQuery(email), ct);
            var user = userResult.Value;

            if (user == null || !user.IsActive)
            {
                passwordHasher.Verify(request.Request.Password, DummyHash);
                await LogFailedAttempt(email, request.IpAddress, ct);
                return Result<AuthResponse>.Failure("Invalid email or password");
            }

            if (!passwordHasher.Verify(request.Request.Password, user.PasswordHash))
            {
                await LogFailedAttempt(email, request.IpAddress, ct);
                return Result<AuthResponse>.Failure("Invalid email or password");
            }

            await unitOfWork.BeginTransactionAsync(ct);
            Result<string> refreshTokenResult;
            try
            {
                if (!string.IsNullOrEmpty(request.Request.FcmToken) && !string.IsNullOrEmpty(request.Request.DeviceId))
                {
                    await sender.Send(new UpdateFcmTokenCommand(user.Id, request.Request.DeviceId, request.Request.FcmToken), ct);
                }

                refreshTokenResult = await sender.Send(new IssueUserRefreshTokenCommand(user.Id, request.IpAddress ?? "Unknown"), ct);
                await sender.Send(new RecordLoginAttemptCommand(email, true, request.IpAddress ?? "Unknown"), ct);

                await unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            var roleNames = user.RoleNames;

            // Phone and image are carried here, not just in the response body: a driver's
            // token is what OrdersService snapshots onto an order they claim, so anything
            // dropped here leaves the customer's driver card incomplete.
            var userForJwt = new User
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageUrl = user.ImageUrl
            };

            // Without this a driver's login token has no applicationStatus claim, so the
            // DriverApproved policy rejects them until they happen to refresh - refresh
            // being the only other path that mints a token, and the only one that passed it.
            var driverStatus = user.DriverStatus?.ToString().ToUpperInvariant();

            var accessToken = jwtService.GenerateAccessToken(userForJwt, roleNames, driverStatus);

            var userDto = new UserDto(
                Id: user.Id,
                Email: user.Email,
                Phone: user.PhoneNumber,
                Name: $"{user.FirstName} {user.LastName}",
                Roles: roleNames,
                CreatedAt: user.CreatedAt,
                UpdatedAt: user.CreatedAt, // Binding UpdatedAt to CreatedAt
                Gender: user.Gender.ToString().ToUpper(),
                NotificationStatus: user.NotificationStatus.ToString().ToUpper()
            );

            var response = new AuthResponse(userDto, accessToken, refreshTokenResult.Value);
            return Result<AuthResponse>.Success(response);
        }

        private async Task LogFailedAttempt(string email, string? ipAddress, CancellationToken ct)
            => await sender.Send(new RecordLoginAttemptCommand(email, false, ipAddress ?? "Unknown"), ct);
    }
}
