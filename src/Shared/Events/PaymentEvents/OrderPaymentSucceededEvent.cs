namespace Shared.Events.PaymentEvents
{
    /// Published when a Stripe checkout session completes successfully.
    /// Consumers MUST configure `UseMessageRetry` (e.g., 3 attempts, 5s interval) to absorb transient failures.
    /// Consumers MUST be implemented idempotently (e.g., ignore if order already paid).

    public record OrderPaymentSucceededEvent
    {
        public Guid OrderId { get; init; }
        public Guid PaymentAttemptId { get; init; }
        public long AmountTotal { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? CustomerEmail { get; init; }
    }
}
