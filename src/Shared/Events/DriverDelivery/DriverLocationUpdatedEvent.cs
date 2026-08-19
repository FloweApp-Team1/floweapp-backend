namespace Shared.Events.DriverDelivery
{
    // Raised by OrdersService whenever a driver ping moves the tracked position
    // meaningfully. NotificationService consumes it to send the customer a silent
    // (data-only) push so the tracking map can move its marker without a refresh.
    public record DriverLocationUpdatedEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        Guid DriverId,
        double Lat,
        double Lng,
        DateTime RecordedAt);
}
