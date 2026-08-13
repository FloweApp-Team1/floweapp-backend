using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Products.ArchiveProduct;

public class ArchiveProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/products/{id:guid}", (Guid id) =>
                ApiResponse.Success(new { }, "Product archived").ToHttpResult())
            .WithTags("Admin")
            .WithName("ArchiveProduct")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
