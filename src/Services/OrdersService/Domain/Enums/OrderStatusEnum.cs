namespace OrdersService.Domain.Enums
{
    // Mirrors the OrderStatus schema in the API contract. Transitions
    // (Placed -> Preparing -> PickedUp -> OutForDelivery -> AwaitingDeliveryConfirmation
    // -> Delivered, or -> Cancelled) are enforced server-side - see UpdateOrderStatus.
    public enum OrderStatusEnum
    {
        Placed,
        Preparing,
        PickedUp,
        OutForDelivery,
        // The driver has reached the destination and handed the order over; the delivery is
        // not Delivered until it is confirmed. Inserted before Delivered rather than appended
        // so the enum reads in delivery order - the column stores the name, not the ordinal,
        // so existing rows are unaffected (see OrderConfiguration / OrderStatusHistoryConfiguration).
        AwaitingDeliveryConfirmation,
        Delivered,
        Cancelled
    }

    public static class OrderStatusExtensions
    {
        // The stages the customer-facing tracking timeline renders, in order. PickedUp is
        // folded into OutForDelivery on the client, but both are live-tracking states here.
        // AwaitingDeliveryConfirmation is deliberately NOT a stage of its own: the customer
        // timeline treats it as a sub-state of Out for Delivery (GetOrderTrackingHandler
        // folds it), so adding it to this array would change the timeline's length for every
        // client. It still shows a live map - see IsLiveDelivery.
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
            status is OrderStatusEnum.PickedUp
                or OrderStatusEnum.OutForDelivery
                or OrderStatusEnum.AwaitingDeliveryConfirmation;

        // Still waiting for someone to carry it. A driver may claim an order in these
        // statuses; everything later either already has a driver or is finished.
        public static bool IsClaimable(this OrderStatusEnum status) =>
            status is OrderStatusEnum.Placed or OrderStatusEnum.Preparing;

        // Everything before the order reaches a terminal state. Tracking is available here;
        // it just may not have a driver or a position yet.
        public static bool IsActiveDelivery(this OrderStatusEnum status) =>
            status is not (OrderStatusEnum.Delivered or OrderStatusEnum.Cancelled);

        // Required mapping for frontend display consistency. PickedUp and
        // AwaitingDeliveryConfirmation fold into Out for Delivery.
        public static string ToDisplayString(this OrderStatusEnum status) => status switch
        {
            OrderStatusEnum.Placed => "Placed",
            OrderStatusEnum.Preparing => "Preparing",
            OrderStatusEnum.PickedUp => "Out for Delivery",
            OrderStatusEnum.OutForDelivery => "Out for Delivery",
            OrderStatusEnum.AwaitingDeliveryConfirmation => "Out for Delivery",
            OrderStatusEnum.Delivered => "Delivered",
            OrderStatusEnum.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }
}
