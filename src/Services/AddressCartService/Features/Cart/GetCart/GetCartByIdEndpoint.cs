using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Cart.GetCart
{
    public class GetCartByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/users/me/carts/{cartId:guid}", async (
                    Guid cartId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetCartQuery(cartId);
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToMinimalApiResult("Cart retrieved");
                })
                .WithTags("Cart")
                .WithName("GetCartById")
                .RequireAuthorization(AppPolicies.CustomerOnly);
        }
    }
}
