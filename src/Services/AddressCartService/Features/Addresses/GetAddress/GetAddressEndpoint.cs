using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Addresses.GetAddress;

public class GetAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/addresses/{addressId:guid}", (Guid addressId) =>
                ApiResponse.Success(new { }, "Address retrieved").ToHttpResult())
            .WithTags("Addresses")
            .WithName("GetAddressDetails")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
