using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Addresses.GetAddresses;

public class GetAddressesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/addresses", async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAddressesQuery(), cancellationToken);

                return result.ToMinimalApiResult("Addresses retrieved");
            })
            .WithTags("Addresses")
            .WithName("GetAddresses")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
