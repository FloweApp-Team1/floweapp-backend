namespace Shared.Events.OrderEvents
{
    public record OrderPlacedEvent
    {
        public Guid OrderId { get; init; }
        public Guid UserId { get; init; }
    }
}
