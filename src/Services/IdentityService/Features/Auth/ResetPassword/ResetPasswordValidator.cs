using FluentValidation;
using Shared.Validation;

namespace IdentityService.Features.Auth.ResetPassword
{
    public sealed class ResetPasswordValidator
        : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.OtpToken)
                .NotEmpty();

            // Same policy registration will use.
            RuleFor(x => x.Password)
                .Password();

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");
        }
    }
}
