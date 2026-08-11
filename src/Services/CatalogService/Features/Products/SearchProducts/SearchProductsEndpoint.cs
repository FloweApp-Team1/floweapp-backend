using Shared.Contracts;
using Shared.Responses;

namespace CatalogService.Features.Products.SearchProducts;

public class SearchProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/search", () =>
                ApiResponse.Success(new { }, "Search results retrieved").ToHttpResult())
            .WithTags("Products")
            .WithName("SearchProducts")
            .AllowAnonymous();
    }
}
