using FluentValidation;

namespace CatalogService.Features.Admin.Categories.UpdateCategory
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".svg" };

        public UpdateCategoryValidator()
        {
            RuleFor(x => x.CategoryId).NotEmpty();

            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => x.Name is not null);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.")
                .When(x => x.Order.HasValue);

            RuleFor(x => x.Icon)
                .Must(f => f!.Length <= 2 * 1024 * 1024)
                .WithMessage("Icon must be 2MB or smaller.")
                .Must(f => AllowedExtensions.Contains(Path.GetExtension(f!.FileName).ToLowerInvariant()))
                .WithMessage("Icon must be jpg, jpeg, png, webp or svg.")
                .When(x => x.Icon is not null);
        }
    }
}
