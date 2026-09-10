using MediatR;
using Shared.Results;

namespace IdentityService.Features.Users.UpdateDeviceNotificationPreference;

public sealed record UpdateDeviceNotificationPreferenceCommand(
    string DeviceId,
    bool? Enabled) : IRequest<Result<UpdateDeviceNotificationPreferenceResponse>>;
