using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Cart.UpdateCartItem;

public class UpdateCartItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/cart/items/{itemId:guid}", (Guid itemId) =>
                ApiResponse.Success(new { }, "Cart item updated").ToHttpResult())
            .WithTags("Cart")
            .WithName("UpdateCartItem")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
