using MediatR;
using Shared.Results;

namespace CatalogService.Features.Products.BatchGetProducts
{
    public sealed record BatchGetProductsQuery(IReadOnlyList<Guid> ProductIds, Guid? StoreId)
        : IRequest<Result<BatchGetProductsResponse>>;
}
