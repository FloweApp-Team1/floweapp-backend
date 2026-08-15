using FluentValidation;

namespace CatalogService.Features.Products.SearchProducts
{
    public sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
    {
        public SearchProductsQueryValidator()
        {
            RuleFor(x => x.Q)
                .NotEmpty().WithMessage("Search query is required.")
                .MaximumLength(100).WithMessage("Search query is too long.");

          
            RuleFor(x => x.Sort)
                .IsInEnum()
                .When(x => x.Sort.HasValue)
                .WithMessage("Unrecognized sort option.");
        }
    }
}
