using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Addresses.DeleteAddress;

public class DeleteAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/addresses/{addressId:guid}", (Guid addressId) =>
                ApiResponse.Success(new { }, "Address deleted").ToHttpResult())
            .WithTags("Addresses")
            .WithName("DeleteAddress")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
