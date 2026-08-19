using CatalogService.Common.Sorting;
using MediatR;
using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;

namespace CatalogService.Features.Products.SearchProducts;

public class SearchProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
       
        app.MapGet("/products/search", async (
                string q,
                ProductSortOption? sort,
                [AsParameters] PaginationRequest pagination,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
         
            var result = await sender.Send(new SearchProductsQuery(q, sort, pagination), cancellationToken);
            return result.ToHttpResult();
        })
            .WithName("SearchProducts")
            .WithTags("Products")
            .Produces<ApiResponse<IReadOnlyList<ProductListItemDto>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

    }
}
