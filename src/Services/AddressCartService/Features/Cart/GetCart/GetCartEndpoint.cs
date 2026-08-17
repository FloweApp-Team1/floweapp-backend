using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Cart.GetCart;

public class GetCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/cart", () =>
                ApiResponse.Success(new { }, "Cart retrieved").ToHttpResult())
            .WithTags("Cart")
            .WithName("GetCart")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
