namespace OrdersService.Domain.Enums
{
    // Mirrors the OrderStatus schema in the API contract. Transitions
    // (Placed -> Preparing -> PickedUp -> OutForDelivery -> Arrived
    // -> AwaitingDeliveryConfirmation -> Delivered, or -> Cancelled) are enforced
    // server-side - see UpdateOrderStatus.
    public enum OrderStatusEnum
    {
        Placed,
        Preparing,
        PickedUp,
        OutForDelivery,
        // The driver has reached the delivery destination but has not yet handed the order
        // over and requested confirmation from the customer.
        Arrived,
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
        // Minimal API query-string enum binding does not use the JSON enum converter.
        // Accept both CLR names (OutForDelivery) and public contract names
        // (OUT_FOR_DELIVERY), case-insensitively.
        public static bool TryParseContract(string? value, out OrderStatusEnum status)
        {
            status = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim().Replace("_", string.Empty);

            foreach (var candidate in Enum.GetValues<OrderStatusEnum>())
            {
                if (string.Equals(
                        candidate.ToString(),
                        normalized,
                        StringComparison.OrdinalIgnoreCase))
                {
                    status = candidate;
                    return true;
                }
            }

            return false;
        }

        // The stages the customer-facing tracking timeline renders, in lifecycle order.
        // Every distinct contract status is represented so IsCurrent and IsCompleted never
        // have to fold one status into another.
        public static readonly OrderStatusEnum[] TimelineStages =
        [
            OrderStatusEnum.Placed,
            OrderStatusEnum.Preparing,
            OrderStatusEnum.PickedUp,
            OrderStatusEnum.OutForDelivery,
            OrderStatusEnum.Arrived,
            OrderStatusEnum.AwaitingDeliveryConfirmation,
            OrderStatusEnum.Delivered
        ];

        // The live map is useful while the driver is carrying the order. Once confirmation
        // is requested, the order has been handed over and location tracking stops.
        public static bool IsLiveDelivery(this OrderStatusEnum status) =>
            status is OrderStatusEnum.PickedUp
                or OrderStatusEnum.OutForDelivery
                or OrderStatusEnum.Arrived;

        // Still waiting for someone to carry it. A driver may claim an order in these
        // statuses; everything later either already has a driver or is finished.
        public static bool IsClaimable(this OrderStatusEnum status) =>
            status is OrderStatusEnum.Placed or OrderStatusEnum.Preparing;

        // Customer-facing labels for status summaries.
        public static string ToDisplayString(this OrderStatusEnum status) => status switch
        {
            OrderStatusEnum.Placed => "Placed",
            OrderStatusEnum.Preparing => "Preparing",
            OrderStatusEnum.PickedUp => "Out for Delivery",
            OrderStatusEnum.OutForDelivery => "Out for Delivery",
            OrderStatusEnum.Arrived => "Arrived",
            OrderStatusEnum.AwaitingDeliveryConfirmation => "Awaiting Delivery Confirmation",
            OrderStatusEnum.Delivered => "Delivered",
            OrderStatusEnum.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }
}
