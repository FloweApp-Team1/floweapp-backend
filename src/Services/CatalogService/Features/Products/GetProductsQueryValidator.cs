using FluentValidation;

namespace CatalogService.Features.Products
{
    public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
    {
        public GetProductsQueryValidator()
        {
            
            RuleFor(x => x.Sort)
                .IsInEnum()
                .When(x => x.Sort.HasValue)
                .WithMessage("Unrecognized sort option.");

          
        }
    }
}
