using MediatR;
using Shared.Contracts;
using Shared.Extensions;

namespace OrdersService.Features.Checkout.GetCheckoutDetails
{
    public sealed class GetCheckoutDetailsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/checkout/details", async (
                    Guid cartId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCheckoutDetailsQuery(cartId), cancellationToken);
                return result.ToMinimalApiResult();
            })
                .RequireAuthorization()
                .WithName("GetCheckoutDetails")
                .WithTags("Checkout")
                .WithSummary("Read-only checkout screen preview: pricing, ETA, and available payment methods for the user's default/last-used address.");
        }
    }
}
