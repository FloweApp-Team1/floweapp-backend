using OrdersService.Domain.Enums;

namespace OrdersService.Domain.Entities
{
    public class OrderStatusHistory : OrdersBaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatusEnum Status { get; set; }

        public DateTime OccurredAt { get; set; }
        public Guid? ChangedBy { get; set; }

        // Free-text context for transitions that need it, most obviously a cancellation
        // reason. Never rendered as the stage label.
        public string? Note { get; set; }
    }
}
