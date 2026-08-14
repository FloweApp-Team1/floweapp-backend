using FluentValidation;

namespace CatalogService.Features.Admin.Occasions.CreateOccasion
{
    public class CreateOccasionValidator : AbstractValidator<CreateOccasionCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public CreateOccasionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Occasion name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.");

            RuleFor(x => x.Image)
                .Must(f => f!.Length <= 3 * 1024 * 1024)
                .WithMessage("Image must be 3MB or smaller.")
                .Must(f => AllowedExtensions.Contains(Path.GetExtension(f!.FileName).ToLowerInvariant()))
                .WithMessage("Image must be jpg, jpeg, png or webp.")
                .When(x => x.Image is not null);
        }
    }
}
