using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Cart.AddCartItem;

public class AddCartItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/cart/items", () =>
                ApiResponse.Success(new { }, "Item added to cart").ToHttpResult())
            .WithTags("Cart")
            .WithName("AddCartItem")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
