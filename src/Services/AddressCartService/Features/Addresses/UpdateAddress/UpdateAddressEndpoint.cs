using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Addresses.UpdateAddress;

public class UpdateAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/addresses/{addressId:guid}", (Guid addressId) =>
                ApiResponse.Success(new { }, "Address updated").ToHttpResult())
            .WithTags("Addresses")
            .WithName("UpdateAddress")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
