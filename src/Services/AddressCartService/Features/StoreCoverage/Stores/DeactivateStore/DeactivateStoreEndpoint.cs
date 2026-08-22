using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.DeactivateStore;

public class DeactivateStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/stores/{storeId:guid}", async (Guid storeId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeactivateStoreCommand(storeId), cancellationToken);
            return result.ToMinimalApiResult("Store deactivated");
        })
            .WithTags("Admin - Stores")
            .WithName("DeactivateStore")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
