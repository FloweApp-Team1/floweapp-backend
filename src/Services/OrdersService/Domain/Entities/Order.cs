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
    }
}
