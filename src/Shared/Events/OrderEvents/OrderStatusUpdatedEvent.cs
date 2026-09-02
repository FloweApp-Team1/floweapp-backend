namespace Shared.Events.OrderEvents
{
    public record OrderStatusUpdatedEvent(
        Guid OrderId,
        Guid CustomerId,
        string OldStatus,
        string NewStatus,
        DateTime Timestamp
    );
}
