using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Cart.GetCart
{
    public class GetCartEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/cart", async (
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetCartQuery();
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToMinimalApiResult("Cart retrieved");
                })
                .WithTags("Cart")
                .WithName("GetCart")
                .RequireAuthorization(AppPolicies.CustomerOnly);
        }
    }
}
