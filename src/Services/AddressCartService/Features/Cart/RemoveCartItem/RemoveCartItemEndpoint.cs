using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace AddressCartService.Features.Cart.RemoveCartItem;

public class RemoveCartItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/cart/items/{itemId:guid}", (Guid itemId) =>
                ApiResponse.Success(new { }, "Item removed from cart").ToHttpResult())
            .WithTags("Cart")
            .WithName("RemoveCartItem")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
