using AddressCartService.Features.StoreCoverage.Common.Dtos;
using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.UpdateStore;

public class UpdateStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/stores/{storeId:guid}", async (
                Guid storeId,
                UpdateStoreRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateStoreCommand(storeId, request), cancellationToken);
            return result.ToMinimalApiResult("Store updated");
        })
            .WithTags("Admin - Stores")
            .WithName("UpdateStore")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
