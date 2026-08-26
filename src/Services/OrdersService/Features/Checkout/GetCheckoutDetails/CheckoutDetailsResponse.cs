using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;

namespace OrdersService.Features.Checkout.GetCheckoutDetails
{
    public sealed record CheckoutDetailsResponse(
       Guid CartId,
       Guid? AddressId,
       bool IsServiceable,
       decimal Subtotal,
       decimal DeliveryFee,
       decimal Total,
       DateTime? EstimatedDeliveryAt,
      IReadOnlyList<PaymentMethodOption> PaymentMethods,
       bool IsGift,
       string? GiftRecipientName,
       string? GiftRecipientPhone);
}
