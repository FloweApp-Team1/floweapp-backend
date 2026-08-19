using FluentValidation;

namespace CatalogService.Features.Admin.HomeSections.CreateHomeSection
{
    public class CreateHomeSectionValidator : AbstractValidator<CreateHomeSectionCommand>
    {
        private static readonly string[] AllowedTypes = { "banner", "rail" };
        private static readonly string[] AllowedRules =
            { "MANUAL", "BEST_SELLERS", "NEW_ARRIVALS", "FEATURED", "ON_SALE" };

        public CreateHomeSectionValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty()
                .Must(t => AllowedTypes.Contains(t.ToLowerInvariant()))
                .WithMessage($"Type must be one of: {string.Join(", ", AllowedTypes)}.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.");

            When(x => x.Type.ToLowerInvariant() == "banner", () =>
            {
                RuleFor(x => x.BannerImageUrl)
                    .NotEmpty().WithMessage("bannerImageUrl is required for a banner section.");
            });

            When(x => x.Type.ToLowerInvariant() == "rail", () =>
            {
                RuleFor(x => x.ProductSelectionRule)
                    .NotEmpty().WithMessage("productSelectionRule is required for a rail section.")
                    .Must(r => AllowedRules.Contains(r!.ToUpperInvariant()))
                    .WithMessage($"productSelectionRule must be one of: {string.Join(", ", AllowedRules)}.")
                    .When(x => x.ProductSelectionRule is not null);

                RuleFor(x => x.ProductIds)
                    .NotEmpty()
                    .WithMessage("productIds is required when productSelectionRule is MANUAL.")
                    .When(x => string.Equals(x.ProductSelectionRule, "MANUAL", StringComparison.OrdinalIgnoreCase));
            });
        }
    }
}
