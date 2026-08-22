using OrdersService.Domain.Enums;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed record PlaceOrderResponse(
        Guid OrderId,
        string OrderNumber,
        OrderStatusEnum Status,
        PaymentStatusEnum PaymentStatus,
        PaymentMethodEnum PaymentMethod,
        decimal Subtotal,
        decimal DeliveryFee,
        decimal Total);
}
