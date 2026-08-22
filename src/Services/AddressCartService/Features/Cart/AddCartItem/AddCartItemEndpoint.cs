using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Cart.AddCartItem
{
    public class AddCartItemEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/cart/items", async (
                    [FromBody] AddCartItemRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new AddCartItemCommand(request.ProductId, request.Quantity);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToMinimalApiResult("Item added to cart");
                })
                .WithTags("Cart")
                .WithName("AddCartItem")
                .RequireAuthorization(AppPolicies.CustomerOnly);
        }
    }
}
