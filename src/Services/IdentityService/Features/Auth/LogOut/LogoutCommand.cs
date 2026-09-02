using IdentityService.Common;
using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Auth.LogOut
{
    public sealed record LogoutCommand(string RefreshToken, string? DeviceId) : IRequest<Result>;

    public sealed class LogoutHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
        : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
        {
            var tokenRepo = unitOfWork.Repository<RefreshToken>();

            // Only the hash is stored, so the incoming raw value has to be hashed to match.
            var hashedToken = jwtService.HashRefreshTokenValue(request.RefreshToken);
            var token = await tokenRepo.FirstOrDefaultAsync(t => t.Token == hashedToken, ct);

            if (token is null)
                return Result.Failure(AuthErrors.RefreshTokenNotFound);

            if (token.RevokedAt is not null)
                return Result.Failure(AuthErrors.RefreshTokenAlreadyRevoked);

            if (token.ExpiresAt <= DateTime.UtcNow)
                return Result.Failure(AuthErrors.RefreshTokenExpired);

            token.RevokedAt = DateTime.UtcNow;
            tokenRepo.Update(token);

            if (!string.IsNullOrEmpty(request.DeviceId))
            {
                var deviceTokenRepo = unitOfWork.Repository<UserDeviceToken>();
                var deviceToken = await deviceTokenRepo.FirstOrDefaultAsync(x => x.UserId == token.UserId && x.DeviceId == request.DeviceId, ct);
                if (deviceToken != null)
                {
                    deviceTokenRepo.Remove(deviceToken);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
