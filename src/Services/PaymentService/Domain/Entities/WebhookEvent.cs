namespace PaymentService.Domain.Entities
{
    public class WebhookEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string StripeEventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTimeOffset ReceivedAt { get; set; }
        public string Payload { get; set; } = string.Empty;
        public bool Processed { get; set; }
    }
}
