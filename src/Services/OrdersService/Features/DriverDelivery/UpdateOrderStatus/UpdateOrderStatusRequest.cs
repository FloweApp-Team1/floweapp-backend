namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    // Body of PATCH /orders/{orderId}/status. Status is DriverStatusUpdate, not the full
    // OrderStatusEnum: it arrives in the contract's SCREAMING_SNAKE_CASE spelling (e.g.
    // "OUT_FOR_DELIVERY"). A value that is not a driver's to set - "PLACED", "PREPARING",
    // "CANCELLED" - is not in the enum, so it fails to bind and is rejected with 400.
    public record UpdateOrderStatusRequest(
        DriverStatusUpdate Status,
        // Optional context stored with the history entry. Never shown as the stage label
        // on the tracking timeline.
        string? Note = null);
}
