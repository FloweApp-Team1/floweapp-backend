using FluentValidation;

namespace CatalogService.Features.Admin.Products.CreateProduct
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(150);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required.")
                .MaximumLength(2000);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.DiscountPercent)
                .InclusiveBetween(0, 100)
                .When(x => x.DiscountPercent.HasValue)
                .WithMessage("Discount percent must be between 0 and 100.");

            RuleFor(x => x.CategoryIds)
                .NotEmpty().WithMessage("At least one category is required.");

            RuleForEach(x => x.StoreStock)
                .Must(s => s.Quantity >= 0)
                .WithMessage("Store stock quantity cannot be negative.");

            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count <= 10)
                .WithMessage("A product can have at most 10 images.");

            RuleForEach(x => x.Images)
                .Must(f => f.Length <= 5 * 1024 * 1024)
                .WithMessage("Each image must be 5MB or smaller.")
                .Must(f => new[] { ".jpg", ".jpeg", ".png", ".webp" }
                    .Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                .WithMessage("Images must be jpg, jpeg, png or webp.");
        }
    }
}
