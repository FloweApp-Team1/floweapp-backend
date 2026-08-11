using Shared.Contracts;
using Shared.Responses;

namespace CatalogService.Features.Products.ListProducts;

public class ListProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", () =>
                ApiResponse.Success(new { }, "Products retrieved").ToHttpResult())
            .WithTags("Products")
            .WithName("ListProducts")
            .AllowAnonymous();
    }
}
