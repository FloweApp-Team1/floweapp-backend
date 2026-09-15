using OrdersService.Domain.Enums;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed record PlaceOrderResponse(
        Guid OrderId,
        string? Status = null,
        string? Gateway = null,
        string? SessionId = null,
        string? SessionUrl = null,
        string? SuccessUrl = null,
        string? CancelUrl = null,
        DateTime? ExpiresAt = null,
        decimal? Amount = null,
        string? Currency = null,
        DateTime? EstimatedDeliveryAt = null);
}
