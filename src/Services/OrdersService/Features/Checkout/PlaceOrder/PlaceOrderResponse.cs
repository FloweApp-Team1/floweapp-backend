using OrdersService.Domain.Enums;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed record PlaceOrderResponse(
        Guid OrderId,
        string Status,
        string Gateway,
        string SessionId,
        string SessionUrl,
        string SuccessUrl,
        string CancelUrl,
        DateTime ExpiresAt,
        decimal Amount,
        string Currency,
        DateTime EstimatedDeliveryAt);
}
