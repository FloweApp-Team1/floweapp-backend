namespace Shared.Events.OrderEvents
{
    public record OrderPaymentFailedEvent
    {
        public Guid OrderId { get; init; }
        public Guid PaymentAttemptId { get; init; }
    }
}
