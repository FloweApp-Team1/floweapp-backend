using Shared.Contracts;
using Shared.Responses;

namespace CatalogService.Features.Products.GetProductDetails;

public class GetProductDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", (Guid id) =>
                ApiResponse.Success(new { }, "Product retrieved").ToHttpResult())
            .WithTags("Products")
            .WithName("GetProductDetails")
            .AllowAnonymous();
    }
}
