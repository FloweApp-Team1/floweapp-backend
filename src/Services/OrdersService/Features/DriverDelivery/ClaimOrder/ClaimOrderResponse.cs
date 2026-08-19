using OrdersService.Domain.Enums;

namespace OrdersService.Features.DriverDelivery.ClaimOrder
{
    public record ClaimOrderResponse(
        Guid OrderId,
        string OrderNumber,
        OrderStatusEnum Status,
        // The same value that lands in Order.DriverAssignedAt and shows on the customer's
        // driver card, so the driver app and the customer app agree on when this started.
        DateTime AssignedAt);
}
