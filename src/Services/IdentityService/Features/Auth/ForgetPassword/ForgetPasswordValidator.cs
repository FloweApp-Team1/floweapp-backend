using FluentValidation;

namespace IdentityService.Features.Auth.ForgetPassword
{
    public sealed class ForgetPasswordValidator
    : AbstractValidator<ForgetPasswordCommand>
    {
        public ForgetPasswordValidator()
        {
            RuleFor(x => x.email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
