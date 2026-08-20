using OrdersService.Domain.Enums;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    // OccurredAt is echoed back so the driver app can show the transition at the time the
    // server recorded it, which is the same time the customer's timeline will show.
    public record UpdateOrderStatusResponse(
        Guid OrderId,
        OrderStatusEnum Status,
        DateTime OccurredAt);
}
