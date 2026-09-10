using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Auth.Login.Commands
{
    public sealed record UpdateFcmTokenCommand(Guid UserId, string DeviceId, string? FcmToken) : IRequest<Result<bool>>;

    public sealed class UpdateFcmTokenHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateFcmTokenCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateFcmTokenCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return Result<bool>.Success(true);

            var repository = unitOfWork.Repository<UserDeviceToken>();
            var token = await repository.Query()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.DeviceId == request.DeviceId, ct);

            if (token == null)
            {
                // A device preference is stored alongside its FCM token. If notification
                // permission has not produced a token yet, report the default without
                // creating an incomplete device row.
                if (string.IsNullOrWhiteSpace(request.FcmToken))
                    return Result<bool>.Success(true);

                token = new UserDeviceToken
                {
                    UserId = request.UserId,
                    DeviceId = request.DeviceId,
                    FcmToken = request.FcmToken,
                    NotificationsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await repository.AddAsync(token, ct);
            }
            else if (!string.IsNullOrWhiteSpace(request.FcmToken))
            {
                token.FcmToken = request.FcmToken;
                token.UpdatedAt = DateTime.UtcNow;
                repository.Update(token);
            }

            return Result<bool>.Success(token.NotificationsEnabled);
        }
    }
}
