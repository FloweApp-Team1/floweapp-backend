using CatalogService.Features.Categories.ListCategories.Dtos;
using MediatR;
using Shared.Requests;
using Shared.Responses;

namespace CatalogService.Features.Categories.ListCategories
{
    public record ListCategoriesQuery(PaginationRequest Pagination)
        : IRequest<ApiResponse<IReadOnlyList<ListCategoriesResponse>>>;
}
