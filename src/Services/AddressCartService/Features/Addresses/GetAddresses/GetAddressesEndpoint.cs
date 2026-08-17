using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Addresses.GetAddresses;

public class GetAddressesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/addresses", () =>
                ApiResponse.Success(new { }, "Addresses retrieved").ToHttpResult())
            .WithTags("Addresses")
            .WithName("GetAddresses")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
