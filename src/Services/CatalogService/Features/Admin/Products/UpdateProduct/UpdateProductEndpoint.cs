using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Products.UpdateProduct;

public class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/products/{id:guid}", (Guid id) =>
                ApiResponse.Success(new { }, "Product updated").ToHttpResult())
            .WithTags("Admin")
            .WithName("UpdateProduct")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
