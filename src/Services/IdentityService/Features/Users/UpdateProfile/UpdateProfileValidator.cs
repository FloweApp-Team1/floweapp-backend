using FluentValidation;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty();

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.ProfilePictureUrl)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));
    }
}