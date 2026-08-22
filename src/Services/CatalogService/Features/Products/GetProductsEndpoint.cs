using CatalogService.Common.Sorting;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
                 Guid? storeId,
                 ProductSortOption? sort,
                [AsParameters] PaginationRequest pagination,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetProductsQuery(
                    CategoryId: categoryId,
                    OccasionId: occasionId,
                    StoreId: storeId,
                    Sort: sort,
                    Pagination: pagination
                );

                var result = await sender.Send(query, cancellationToken);
                return result.ToHttpResult();
            })
                .WithTags("Products")
                .WithName("GetProducts")
                .Produces<ApiResponse<IReadOnlyList<ProductListItemDto>>>(StatusCodes.Status200OK)
                .AllowAnonymous(); 
        }
    }
}