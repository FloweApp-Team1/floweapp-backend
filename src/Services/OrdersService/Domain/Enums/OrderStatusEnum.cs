namespace OrdersService.Domain.Enums
{
    // Mirrors the OrderStatus schema in the API contract. Transitions
    // (Placed -> Preparing -> PickedUp -> OutForDelivery -> Delivered, or -> Cancelled)
    // are enforced server-side - see UpdateOrderStatus.
    public enum OrderStatusEnum
    {
        Placed,
        Preparing,
        PickedUp,
        OutForDelivery,
        Delivered,
        Cancelled
    }

    public static class OrderStatusExtensions
    {
        // The stages the customer-facing tracking timeline renders, in order. PickedUp is
        // folded into OutForDelivery on the client, but both are live-tracking states here.
        public static readonly OrderStatusEnum[] TimelineStages =
        [
            OrderStatusEnum.Placed,
            OrderStatusEnum.Preparing,
            OrderStatusEnum.PickedUp,
            OrderStatusEnum.OutForDelivery,
            OrderStatusEnum.Delivered
        ];

        // A driver is physically carrying the order, so a live map is worth showing and
        // location pings are worth persisting. Delivered/Cancelled orders get a static
        // summary instead - see GetOrderTrackingHandler.
        public static bool IsLiveDelivery(this OrderStatusEnum status) =>
            status is OrderStatusEnum.PickedUp or OrderStatusEnum.OutForDelivery;

        // Still waiting for someone to carry it. A driver may claim an order in these
        // statuses; everything later either already has a driver or is finished.
        public static bool IsClaimable(this OrderStatusEnum status) =>
            status is OrderStatusEnum.Placed or OrderStatusEnum.Preparing;

        // Everything before the order reaches a terminal state. Tracking is available here;
        // it just may not have a driver or a position yet.
        public static bool IsActiveDelivery(this OrderStatusEnum status) =>
            status is not (OrderStatusEnum.Delivered or OrderStatusEnum.Cancelled);
    }
}
