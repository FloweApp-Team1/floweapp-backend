namespace Shared.Events.PaymentEvents
{
    /// <summary>
    /// Published when a Stripe checkout session expires or an async payment fails.
    /// 
    /// **IMPORTANT FOR CONSUMERS:**
    /// 1. Consumers MUST configure `UseMessageRetry` (e.g., 3 attempts, 5s interval) to absorb transient failures.
    /// 2. Consumers MUST be implemented idempotently (e.g., ignore if order is already marked failed or paid).
    /// </summary>
    public record OrderPaymentFailedEvent
    {
        public Guid OrderId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
