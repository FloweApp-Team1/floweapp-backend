using FluentValidation;

namespace CatalogService.Features.Categories.ListCategories
{
    public class ListCategoriesQueryValidator : AbstractValidator<ListCategoriesQuery>
    {
        public ListCategoriesQueryValidator()
        {
            RuleFor(x => x.Pagination.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.Pagination.PageSize)
                .InclusiveBetween(1, 100);
        }
    }
}
