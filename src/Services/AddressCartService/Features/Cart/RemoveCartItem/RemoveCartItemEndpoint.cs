using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Cart.RemoveCartItem
{
    public class RemoveCartItemEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/cart/items/{itemId:guid}", async (
                    Guid itemId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RemoveCartItemCommand(itemId);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToMinimalApiResult("Item removed from cart");
                })
                .WithTags("Cart")
                .WithName("RemoveCartItem")
                .RequireAuthorization(AppPolicies.CustomerOnly);
        }
    }
}
