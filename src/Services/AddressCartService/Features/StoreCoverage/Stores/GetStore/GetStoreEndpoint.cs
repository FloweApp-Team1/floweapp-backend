using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.Stores.GetStore;

public class GetStoreEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/stores/{storeId:guid}", async (Guid storeId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetStoreQuery(storeId), cancellationToken);
            return result.ToMinimalApiResult("Store retrieved");
        })
            .WithTags("Admin - Stores")
            .WithName("GetStoreDetails")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
