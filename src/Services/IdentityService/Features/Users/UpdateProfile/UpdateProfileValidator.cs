using FluentValidation;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);


        RuleFor(x => x.PhoneNumber)
            .NotEmpty();

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.ProfilePictureUrl)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));
    }
}