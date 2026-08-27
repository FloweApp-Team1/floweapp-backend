using MediatR;
using OrdersService.Domain.Enums;
using OrdersService.Features.Checkout.PlaceOrder;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Results;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed class PlaceOrderEndpoint : IEndpoint
    {
    public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/orders/place", async (
                    PlaceOrderRequest request,
                    HttpContext httpContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
            {
                var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].ToString();
                if (string.IsNullOrWhiteSpace(idempotencyKey))
                    return Result.Failure<PlaceOrderResponse?>(
                        Error.New("Order.MissingIdempotencyKey", "Idempotency-Key header is required.")).ToMinimalApiResult();

                var command = new PlaceOrderCommand(
                    request.CartId,
                    request.AddressId,
                    request.IsGift,
                    request.GiftRecipient,
                    request.PaymentMethod,
                    request.PaymentGateway,
                    idempotencyKey);

                var result = await sender.Send(command, cancellationToken);
                return result.ToMinimalApiResult("Order placed successfully.");
            })
                .RequireAuthorization()
                .WithName("PlaceOrder")
                .WithTags("Checkout")
                .WithSummary("Single entry point for both payment methods. COD returns no data; Card returns a gateway checkout session.");
        }
    }
}

public sealed record PlaceOrderRequest(
    Guid CartId,
    Guid AddressId,
    bool IsGift,
    GiftRecipientRequest? GiftRecipient,
    PaymentMethodEnum PaymentMethod,
    string? PaymentGateway);
