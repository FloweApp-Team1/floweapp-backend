namespace IdentityService.Features.Users.UpdateDeviceNotificationPreference;

public sealed record UpdateDeviceNotificationPreferenceResponse(
    string DeviceId,
    bool NotificationsEnabled);
