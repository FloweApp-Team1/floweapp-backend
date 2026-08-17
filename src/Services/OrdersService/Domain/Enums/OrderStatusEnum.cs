namespace OrdersService.Domain.Enums
{
    // Mirrors the OrderStatus schema in the API contract. Transitions
    // (Placed -> Preparing -> PickedUp -> OutForDelivery -> Delivered, or -> Cancelled)
    // are enforced server-side - see UpdateOrderStatus (not implemented yet).
    public enum OrderStatusEnum
    {
        Placed,
        Preparing,
        PickedUp,
        OutForDelivery,
        Delivered,
        Cancelled
    }
}
