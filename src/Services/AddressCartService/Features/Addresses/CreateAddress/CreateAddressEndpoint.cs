using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Addresses.CreateAddress;

public class CreateAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/addresses", () =>
                ApiResponse.Success(new { }, "Address created").ToHttpResult())
            .WithTags("Addresses")
            .WithName("CreateAddress")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
