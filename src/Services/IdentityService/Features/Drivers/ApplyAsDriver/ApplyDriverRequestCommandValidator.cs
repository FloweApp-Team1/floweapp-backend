using FluentValidation;
using IdentityService.Features.Drivers.Dtos_VM;

namespace IdentityService.Features.Drivers.ApplyAsDriver
{
    public class ApplyDriverRequestCommandValidator:AbstractValidator<ApplyDriverRequestCommand>
    {
        public ApplyDriverRequestCommandValidator()
        {
            RuleFor(x => x.Name)
           .NotEmpty()
           .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Phone)
                .NotEmpty()
                .Matches(@"^01[0125]\d{8}$")
                .WithMessage("Invalid Egyptian phone number.");

            RuleFor(x => x.Gender)
                .IsInEnum();


            RuleFor(x => x.VehiclePlateNumber)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.VehicleType)
                .IsInEnum();

            RuleFor(x => x.VehicleCapacity)
                .GreaterThan(0)
                .LessThanOrEqualTo(20);

            RuleFor(x => x.LicenceImage)
                .NotNull()
                .Must(BeValidImage)
                .WithMessage("Licence image must be a valid JPG, JPEG or PNG file with a maximum size of 5 MB.");

            RuleFor(x => x.Nid)
                .NotEmpty()
                .Length(14)
                .Matches(@"^\d{14}$")
                .WithMessage("National ID must consist of exactly 14 digits.");

            RuleFor(x => x.NidImage)
                .NotNull()
                .Must(BeValidImage)
                .WithMessage("National ID image must be a valid JPG, JPEG or PNG file with a maximum size of 5 MB.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one digit.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");
        }

        private static bool BeValidImage(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            const long maxSize = 5 * 1024 * 1024; // 5 MB

            return allowedExtensions.Contains(extension)
                   && file.Length <= maxSize;
        }
    }
    
}
