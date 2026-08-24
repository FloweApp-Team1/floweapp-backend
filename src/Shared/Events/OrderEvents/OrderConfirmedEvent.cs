namespace Shared.Events.OrderEvents
{
    public record OrderConfirmedEvent
    {
        public Guid OrderId { get; init; }
        public Guid UserId { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public string OrderNumber { get; init; } = string.Empty;
        public decimal Total { get; init; }
        public string? UserEmail { get; init; }
    }
}
