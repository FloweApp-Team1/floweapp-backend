using FluentValidation;

namespace CatalogService.Features.Admin.Occasions.UpdateOccasion
{
    public class UpdateOccasionValidator : AbstractValidator<UpdateOccasionCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public UpdateOccasionValidator()
        {
            RuleFor(x => x.OccasionId).NotEmpty();

            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => x.Name is not null);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.")
                .When(x => x.Order.HasValue);

            RuleFor(x => x.Image)
                .Must(f => f!.Length <= 3 * 1024 * 1024)
                .WithMessage("Image must be 3MB or smaller.")
                .Must(f => AllowedExtensions.Contains(Path.GetExtension(f!.FileName).ToLowerInvariant()))
                .WithMessage("Image must be jpg, jpeg, png or webp.")
                .When(x => x.Image is not null);
        }
    }
}
