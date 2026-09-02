using FluentValidation;

namespace IdentityService.Features.Users.UpdateFcmToken;

public class UpdateFcmTokenValidator : AbstractValidator<UpdateFcmTokenCommand>
{
    public UpdateFcmTokenValidator()
    {
        RuleFor(x => x.FcmToken)
            .NotEmpty()
            .WithMessage("FCM Token is required");

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("Device ID is required");
    }
}
