using Shared.Models;
using Shared.Requests;
using MediatR;
using Shared.Responses;

namespace CatalogService.Features.Occasions.ListOccasions
{
    public record ListOccasionsQuery(PaginationRequest Pagination)
        : IRequest<ApiResponse<PagedResult<ListOccasionsResponse>>>;
}
