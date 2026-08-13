using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Products.CreateProduct;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/products", () =>
                ApiResponse.Success(new { }, "Product created").ToHttpResult())
            .WithTags("Admin")
            .WithName("CreateProduct")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
