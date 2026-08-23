using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.StoreCoverage.ResolveStore
{
    public class ResolveStoreEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/stores/resolve", async (
                    Guid? addressId,
                    double? lat,
                    double? lng,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ResolveStoreQuery(addressId, lat, lng), cancellationToken);

                    return result.ToMinimalApiResult("Store resolved");
                })
                .WithTags("StoreCoverage")
                .WithName("ResolveStore")
                .RequireAuthorization(AppPolicies.CustomerOnly);
        }
    }
}
