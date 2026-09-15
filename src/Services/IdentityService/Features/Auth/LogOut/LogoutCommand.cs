using IdentityService.Common;
using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Auth.LogOut
{
    public sealed record LogoutCommand(Guid UserId, string RefreshToken, string? DeviceId) : IRequest<Result>;

    public sealed class LogoutHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
        : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
        {
            var tokenRepo = unitOfWork.Repository<RefreshToken>();

            // Only the hash is stored, so the incoming raw value has to be hashed to match.
            var hashedToken = jwtService.HashRefreshTokenValue(request.RefreshToken);
            var token = await tokenRepo.FirstOrDefaultAsync(t => t.Token == hashedToken, ct);

            if (token is null || token.UserId != request.UserId)
                return Result.Failure(AuthErrors.RefreshTokenNotFound);

            if (token.RevokedAt is not null)
                return Result.Failure(AuthErrors.RefreshTokenAlreadyRevoked);

            if (token.ExpiresAt <= DateTime.UtcNow)
                return Result.Failure(AuthErrors.RefreshTokenExpired);

            var deviceTokenRepo = unitOfWork.Repository<UserDeviceToken>();
            if (string.IsNullOrWhiteSpace(request.DeviceId))
            {
                var activeSessions = await tokenRepo.Query()
                    .Where(t => t.UserId == request.UserId && t.RevokedAt == null)
                    .ToListAsync(ct);
                foreach (var session in activeSessions)
                {
                    session.RevokedAt = DateTime.UtcNow;
                    tokenRepo.Update(session);
                }

                var devices = await deviceTokenRepo.Query()
                    .Where(d => d.UserId == request.UserId)
                    .ToListAsync(ct);
                foreach (var device in devices)
                    deviceTokenRepo.Remove(device);
            }
            else
            {
                token.RevokedAt = DateTime.UtcNow;
                tokenRepo.Update(token);

                var deviceToken = await deviceTokenRepo.FirstOrDefaultAsync(
                    d => d.UserId == request.UserId && d.DeviceId == request.DeviceId, ct);
                if (deviceToken is not null)
                    deviceTokenRepo.Remove(deviceToken);
            }

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
