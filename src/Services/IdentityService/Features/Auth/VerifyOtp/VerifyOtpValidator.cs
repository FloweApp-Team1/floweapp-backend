using FluentValidation;

namespace IdentityService.Features.Auth.VerifyOtp
{
    public sealed class VerifyOtpValidator
    : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Otp)
                .NotEmpty()
                .Length(6)
                .Matches("^[A-Z0-9]{6}$")
                .WithMessage("OTP must be 6 uppercase letters or numbers.");
        }
    }
}
