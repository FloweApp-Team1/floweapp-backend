using OrdersService.Domain.Enums;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    // The statuses a driver may set through PATCH /orders/{orderId}/status. Deliberately
    // narrower than OrderStatusEnum - Placed, Preparing and Cancelled are not a driver's
    // call, so a body carrying one of those fails to bind and is rejected with 400 before
    // the handler runs. That is a stronger guarantee than a hand-maintained allow-list
    // inside the handler, which only holds as long as someone keeps it in sync with the enum.
    //
    // Delivered IS here: the driver completes the delivery. It is only reachable from
    // AwaitingDeliveryConfirmation though - the transition rules reject a driver trying to
    // jump straight from OutForDelivery to Delivered (409), so the confirmation step cannot
    // be skipped.
    public enum DriverStatusUpdate
    {
        PickedUp,
        OutForDelivery,
        AwaitingDeliveryConfirmation,
        Delivered
    }

    public static class DriverStatusUpdateExtensions
    {
        public static OrderStatusEnum ToOrderStatus(this DriverStatusUpdate status) => status switch
        {
            DriverStatusUpdate.PickedUp => OrderStatusEnum.PickedUp,
            DriverStatusUpdate.OutForDelivery => OrderStatusEnum.OutForDelivery,
            DriverStatusUpdate.AwaitingDeliveryConfirmation => OrderStatusEnum.AwaitingDeliveryConfirmation,
            DriverStatusUpdate.Delivered => OrderStatusEnum.Delivered,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status), status, "Unmapped driver status update.")
        };
    }
}
