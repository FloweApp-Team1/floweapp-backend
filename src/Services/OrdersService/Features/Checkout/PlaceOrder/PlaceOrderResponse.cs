namespace OrdersService.Features.Checkout.PlaceOrder
{
    // One stable response contract for both Card and COD. Fields that do not apply to
    // the selected payment method are serialized as null instead of being omitted.
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
