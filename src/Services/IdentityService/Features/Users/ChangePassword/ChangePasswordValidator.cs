using FluentValidation;
using Shared.Validation;

namespace IdentityService.Features.Users.ChangePassword
{
    public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();

            // Shared policy - see PasswordRules, also used by password reset.
            RuleFor(x => x.NewPassword).Password();

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("New password and confirmation do not match.");

            RuleFor(x => x)
                .Must(x => x.CurrentPassword != x.NewPassword)
                .WithMessage("New password must be different from the current password.");
        }
    }
}
