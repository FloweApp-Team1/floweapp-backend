using FluentValidation;

namespace IdentityService.Features.Auth.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Invalid email or password")
                .EmailAddress().WithMessage("Invalid email or password");

            RuleFor(x => x.Request.Password)
                .NotEmpty().WithMessage("Invalid email or password");
        }
    }
}