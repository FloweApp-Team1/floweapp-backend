namespace OrdersService.Features.DriverDelivery.UpdateDriverLocation
{
    // Echoes what the ping actually did. Broadcast tells the driver app whether this fix
    // reached the customer or was absorbed by the throttle, which makes the silent-push
    // path debuggable from the client side.
    public record UpdateDriverLocationResponse(
        DateTime RecordedAt,
        IReadOnlyList<Guid> UpdatedOrderIds,
        bool Broadcast);
}
