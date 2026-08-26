using MediatR;
using Shared.Contracts;
using Shared.Extensions;

namespace OrdersService.Features.Checkout.GetEstimateDelivery
{
    public sealed class GetEstimateDeliveryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/checkout/estimate-delivery", async (
                    Guid addressId,
                    Guid cartId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetEstimateDeliveryQuery(addressId, cartId), cancellationToken);
                return result.ToMinimalApiResult();
            })
                .RequireAuthorization()
                .WithName("GetEstimateDelivery")
                .WithTags("Checkout")
                .WithSummary("Recomputes the delivery fee and ETA for a different address, given the current cart.");
        }
    }
}
