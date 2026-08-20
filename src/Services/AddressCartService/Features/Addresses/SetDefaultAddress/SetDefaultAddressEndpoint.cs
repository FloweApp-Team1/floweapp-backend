using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Addresses.SetDefaultAddress;

public class SetDefaultAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/addresses/{addressId:guid}/default", (Guid addressId) =>
                ApiResponse.Success(new { }, "Default address updated").ToHttpResult())
            .WithTags("Addresses")
            .WithName("SetDefaultAddress")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
