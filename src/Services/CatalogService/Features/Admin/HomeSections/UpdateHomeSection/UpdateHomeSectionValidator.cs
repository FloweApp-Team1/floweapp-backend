using FluentValidation;

namespace CatalogService.Features.Admin.HomeSections.UpdateHomeSection
{
    public class UpdateHomeSectionValidator : AbstractValidator<UpdateHomeSectionCommand>
    {
        private static readonly string[] AllowedRules =
            { "MANUAL", "BEST_SELLERS", "NEW_ARRIVALS", "FEATURED", "ON_SALE" };

        public UpdateHomeSectionValidator()
        {
            RuleFor(x => x.SectionId).NotEmpty();

            RuleFor(x => x.Title)
                .MaximumLength(150)
                .When(x => x.Title is not null);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.")
                .When(x => x.Order.HasValue);

            RuleFor(x => x.ProductSelectionRule)
                .Must(r => AllowedRules.Contains(r!.ToUpperInvariant()))
                .WithMessage($"productSelectionRule must be one of: {string.Join(", ", AllowedRules)}.")
                .When(x => x.ProductSelectionRule is not null);
        }
    }
}
