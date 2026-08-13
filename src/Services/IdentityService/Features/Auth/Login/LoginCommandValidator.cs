using FluentValidation;

namespace IdentityService.Features.Auth.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        private const string UppercasePattern = @"[A-Z]";
        private const string DigitPattern = @"\d";

        public LoginCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Invalid email or password")
                .EmailAddress().WithMessage("Invalid email or password");

            RuleFor(x => x.Request.Password)
                .NotEmpty().WithMessage("Invalid email or password")
                .MinimumLength(6).WithMessage("Invalid email or password")
                .Matches(UppercasePattern).WithMessage("Invalid email or password")
                .Matches(DigitPattern).WithMessage("Invalid email or password");
        }
    }
}
