using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Cart.UpdateCartItem
{
    public class UpdateCartItemEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("/cart/items/{itemId:guid}", async (
                    Guid itemId,
                    [FromBody] UpdateCartItemRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCartItemCommand(itemId, request.Quantity);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToMinimalApiResult("Cart item updated");
                })
                .WithTags("Cart")
                .WithName("UpdateCartItem")
                .RequireAuthorization(AppPolicies.CustomerOnly);
        }
    }
}
