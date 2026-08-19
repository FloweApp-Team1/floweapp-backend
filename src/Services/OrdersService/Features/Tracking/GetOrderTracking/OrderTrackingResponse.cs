using OrdersService.Domain.Enums;

namespace OrdersService.Features.Tracking.GetOrderTracking
{
    // Everything the Order Tracking screen needs in one read. Each section is independently
    // nullable so the client can degrade a piece at a time: no driver yet, no position yet,
    // or no destination coordinates still leaves a renderable status timeline.
    public record OrderTrackingResponse(
        Guid OrderId,
        string OrderNumber,
        OrderStatusEnum Status,
        // False once the order reaches Delivered or Cancelled: the client shows a static
        // summary rather than a live map.
        bool IsTrackingActive,
        IReadOnlyList<OrderTrackingStageDto> Timeline,
        OrderTrackingDriverDto? Driver,
        OrderTrackingLocationDto? LastKnownLocation,
        OrderTrackingDestinationDto? Destination);

    public record OrderTrackingStageDto(
        OrderStatusEnum Status,
        bool IsCompleted,
        bool IsCurrent,
        // When the order entered this stage, from OrderStatusHistory. Null for a stage that
        // has not happened yet, and for one that happened before the order had any history
        // recorded - the client renders the label without a time rather than guessing one.
        DateTime? OccurredAt);

    public record OrderTrackingDriverDto(
        Guid DriverId,
        string? Name,
        string? Phone,
        string? ImageUrl,
        DateTime? AssignedAt);

    public record OrderTrackingLocationDto(
        double Lat,
        double Lng,
        DateTime RecordedAt,
        int AgeSeconds,
        // True once the position is older than the configured staleness threshold. The
        // client keeps the marker on screen but labels it as possibly out of date.
        bool IsStale);

    public record OrderTrackingDestinationDto(
        double? Lat,
        double? Lng,
        string RecipientName,
        string AddressLine,
        string City,
        string Area);
}
