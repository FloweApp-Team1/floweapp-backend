namespace Shared.Events.OrderEvents
{
    public record OrderPaymentSucceededEvent
    {
        public Guid OrderId { get; init; }
        public Guid PaymentAttemptId { get; init; }
    }
}
