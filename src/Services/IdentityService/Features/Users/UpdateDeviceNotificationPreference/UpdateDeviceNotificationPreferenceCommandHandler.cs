using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace IdentityService.Features.Users.UpdateDeviceNotificationPreference;

public sealed class UpdateDeviceNotificationPreferenceCommandHandler(
    ICurrentUserService currentUser,
    IGenericRepository<UserDeviceToken> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDeviceNotificationPreferenceCommand, Result<UpdateDeviceNotificationPreferenceResponse>>
{
    public async Task<Result<UpdateDeviceNotificationPreferenceResponse>> Handle(
        UpdateDeviceNotificationPreferenceCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return Result<UpdateDeviceNotificationPreferenceResponse>.Failure(
                Error.New("Users.Unauthorized", "Unauthorized"));
        }

        var device = await repository.Query()
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.DeviceId == request.DeviceId,
                cancellationToken);

        if (device is null)
        {
            return Result<UpdateDeviceNotificationPreferenceResponse>.Failure(
                Error.New("Devices.NotFound", "Device is not registered for this user"));
        }

        device.NotificationsEnabled = request.Enabled!.Value;
        device.UpdatedAt = DateTime.UtcNow;
        repository.Update(device);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateDeviceNotificationPreferenceResponse>.Success(
            new UpdateDeviceNotificationPreferenceResponse(
                device.DeviceId,
                device.NotificationsEnabled));
    }
}
