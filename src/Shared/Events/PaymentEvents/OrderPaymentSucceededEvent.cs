namespace Shared.Events.PaymentEvents
{
    /// <summary>
    /// Published when a Stripe checkout session completes successfully.
    /// 
    /// **IMPORTANT FOR CONSUMERS:**
    /// 1. Consumers MUST configure `UseMessageRetry` (e.g., 3 attempts, 5s interval) to absorb transient failures.
    /// 2. Consumers MUST be implemented idempotently (e.g., ignore if order already paid).
    /// </summary>
    public record OrderPaymentSucceededEvent
    {
        public Guid OrderId { get; init; }
        public Guid PaymentAttemptId { get; init; }
        public long AmountTotal { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? CustomerEmail { get; init; }
    }
}
