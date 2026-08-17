using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities
{
    public class PaymentAttempt : Shared.Domain.BaseEntity
    {
        public Guid OrderId { get; set; }
        public string StripeSessionId { get; set; } = string.Empty;
        public string? StripePaymentIntentId { get; set; }
        public string? SessionUrl { get; set; }
        public long AmountTotal { get; set; }
        
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public int AttemptNumber { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}
