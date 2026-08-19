using OrdersService.Domain.Enums;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    // Body of PATCH /orders/{orderId}/status. Status arrives in the contract's
    // SCREAMING_SNAKE_CASE spelling (e.g. "OUT_FOR_DELIVERY") - see Program.cs.
    public record UpdateOrderStatusRequest(
        OrderStatusEnum Status,
        // Optional context stored with the history entry, most obviously why the driver
        // cancelled. Never shown as the stage label on the tracking timeline.
        string? Note = null);
}
