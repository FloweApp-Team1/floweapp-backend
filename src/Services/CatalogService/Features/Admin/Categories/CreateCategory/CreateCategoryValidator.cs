using FluentValidation;

namespace CatalogService.Features.Admin.Categories.CreateCategory
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".svg" };

        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.");

            RuleFor(x => x.Icon)
                .Must(f => f!.Length <= 2 * 1024 * 1024)
                .WithMessage("Icon must be 2MB or smaller.")
                .Must(f => AllowedExtensions.Contains(Path.GetExtension(f!.FileName).ToLowerInvariant()))
                .WithMessage("Icon must be jpg, jpeg, png, webp or svg.")
                .When(x => x.Icon is not null);
        }
    }
}
