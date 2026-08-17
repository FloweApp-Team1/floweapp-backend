using CatalogService.Features.Products.ListProducts.Dtos;
using MediatR;
using Shared.Models;
using Shared.Requests;
using Shared.Results;

namespace CatalogService.Features.Products.ListProducts
{
    public record ListProductsQuery(PaginationRequest Pagination,Guid CategoryId) : IRequest<Result<PagedResult<ListProductResponseDto>>>;
}