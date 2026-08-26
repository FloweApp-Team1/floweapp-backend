using OrdersService.Domain.Enums;

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
       IReadOnlyList<PaymentMethodEnum> PaymentMethods,
       bool IsGift,
       string? GiftRecipientName,
       string? GiftRecipientPhone);
}
