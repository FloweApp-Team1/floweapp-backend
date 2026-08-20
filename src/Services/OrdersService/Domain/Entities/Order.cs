using OrdersService.Domain.Enums;

namespace OrdersService.Domain.Entities
{
    public class Order : OrdersBaseEntity
    {
        public string OrderNumber { get; set; } = null!;
        public Guid UserId { get; set; }
        public Guid StoreId { get; set; }
        public Guid? AddressId { get; set; }
        public Guid? DriverId { get; set; }

        // Snapshot of the assigned driver's IdentityService profile, captured at assignment
        // time for the same reason OrderAddressSnapshot exists: Orders must be able to render
        // the tracking screen's driver card without a cross-service read, and the card should
        // keep showing who actually delivered even if the profile later changes.
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverImageUrl { get; set; }
        public DateTime? DriverAssignedAt { get; set; }

        public OrderStatusEnum Status { get; set; } = OrderStatusEnum.Placed;
        public PaymentMethodEnum PaymentMethod { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Pending;

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }

        public bool IsGift { get; set; }
        public string? GiftRecipientName { get; set; }
        public string? GiftRecipientPhone { get; set; }
        public string? GiftRecipientAddress { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public OrderAddressSnapshot? AddressSnapshot { get; set; }

        // Every status this order has passed through, with the time it got there. Status
        // above is only the latest one, which is not enough to date the tracking timeline.
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}