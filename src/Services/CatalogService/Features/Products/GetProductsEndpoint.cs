using CatalogService.Common.Sorting;
using MediatR;
using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;

namespace CatalogService.Features.Products
{
    public sealed class GetProductsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", async (
                    Guid? categoryId,
                    Guid? occasionId,
                    ProductSortOption? sort,
                    [AsParameters] PaginationRequest pagination,
                    ISender sender,
                    CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetProductsQuery(categoryId, occasionId, sort, pagination), cancellationToken);
                return result.ToHttpResult();
            })
                .WithTags("Products")
                .WithName("GetProducts")
                .Produces<ApiResponse<IReadOnlyList<ProductListItemDto>>>(StatusCodes.Status200OK)
                .AllowAnonymous(); 
        }
    }
}