using FluentValidation;

namespace IdentityService.Features.Users.UpdateDeviceNotificationPreference;

public sealed class UpdateDeviceNotificationPreferenceValidator
    : AbstractValidator<UpdateDeviceNotificationPreferenceCommand>
{
    public UpdateDeviceNotificationPreferenceValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Enabled)
            .NotNull()
            .WithMessage("Enabled is required");
    }
}
