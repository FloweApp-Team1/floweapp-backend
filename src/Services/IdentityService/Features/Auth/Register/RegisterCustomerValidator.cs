using FluentValidation;

namespace IdentityService.Features.Auth.Register
{
    public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
    {
        // 01[0-2,5]XXXXXXXX — 11 digits total
        private const string EgyptianPhonePattern = @"^01[0125][0-9]{8}$";

        // Min 6 chars, at least one uppercase letter, at least one digit
        private const string UppercasePattern = @"[A-Z]";
        private const string DigitPattern = @"\d";

        public RegisterCustomerValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(EgyptianPhonePattern)
                .WithMessage("Phone number must be a valid Egyptian mobile number (e.g. 01xxxxxxxxx)");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Gender must be Male or Female");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches(UppercasePattern).WithMessage("Password must contain at least one uppercase letter")
                .Matches(DigitPattern).WithMessage("Password must contain at least one digit");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Passwords do not match");


        }
    }
}
