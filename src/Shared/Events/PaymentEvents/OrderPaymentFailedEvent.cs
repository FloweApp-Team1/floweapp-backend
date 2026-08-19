namespace Shared.Events.PaymentEvents
{
    /// Published when a Stripe checkout session expires or an async payment fails.
    /// Consumers MUST configure `UseMessageRetry` (e.g., 3 attempts, 5s interval) to absorb transient failures.
    /// Consumers MUST be implemented idempotently (e.g., ignore if order is already marked failed or paid).
    public record OrderPaymentFailedEvent
    {
        public Guid OrderId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
