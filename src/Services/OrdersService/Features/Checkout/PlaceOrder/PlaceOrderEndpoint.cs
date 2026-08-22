using MediatR;
using Shared.Contracts;
using Shared.Extensions;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed class PlaceOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/orders", async (
                    PlaceOrderCommand command,
                    ISender sender,
                    CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);
                return result.ToMinimalApiResult("Order placed successfully.");
            })
                .RequireAuthorization()
                .WithName("PlaceOrder")
                .WithTags("Checkout")
                .WithSummary("Creates an order from the user's cart, validating address, store coverage, and totals server-side.");
        }
    }
}