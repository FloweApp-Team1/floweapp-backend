using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace CatalogService.Features.Admin.Products.ArchiveProduct;
public class ArchiveProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/products/{productId:guid}", async (
                Guid productId,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ArchiveProductCommand(productId), cancellationToken);
            return result.ToMinimalApiResult("Product archived");
        })
            .WithTags("Admin")
            .WithName("ArchiveProduct")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
