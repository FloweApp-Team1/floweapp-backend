using FluentValidation;

namespace CatalogService.Features.Admin.Products.UpdateProduct
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();

            RuleFor(x => x.Name)
                .MaximumLength(150)
                .When(x => x.Name is not null);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description is not null);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.DiscountPercent)
                .InclusiveBetween(0, 100)
                .When(x => x.DiscountPercent.HasValue)
                .WithMessage("Discount percent must be between 0 and 100.");

            RuleFor(x => x.CategoryIds)
                .Must(ids => ids == null || ids.Count > 0)
                .WithMessage("At least one category is required when categoryIds is provided.");

            RuleForEach(x => x.StoreStock)
                .Must(s => s.Quantity >= 0)
                .WithMessage("Store stock quantity cannot be negative.")
                .When(x => x.StoreStock is not null);

            RuleForEach(x => x.Images)
                .Must(f => f.Length <= 5 * 1024 * 1024)
                .WithMessage("Each image must be 5MB or smaller.")
                .Must(f => new[] { ".jpg", ".jpeg", ".png", ".webp" }
                    .Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                .WithMessage("Images must be jpg, jpeg, png or webp.")
                .When(x => x.Images is not null);
        }
    }
}
