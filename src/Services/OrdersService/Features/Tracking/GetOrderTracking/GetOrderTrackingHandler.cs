using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;
using OrdersService.Infrastructure.Settings;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.Tracking.GetOrderTracking
{
    // SCRUM-151: the single read behind the Order Tracking screen - current status and
    // timeline, the assigned driver summary when there is one, the last known driver
    // position with its age, and the delivery destination.
    public class GetOrderTrackingHandler
        : IRequestHandler<GetOrderTrackingQuery, Result<OrderTrackingResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDriverLocationCache _locationCache;
        private readonly DeliveryTrackingSettings _settings;

        public GetOrderTrackingHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDriverLocationCache locationCache,
            IOptions<DeliveryTrackingSettings> settings)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _locationCache = locationCache;
            _settings = settings.Value;
        }

        public async Task<Result<OrderTrackingResponse>> Handle(
            GetOrderTrackingQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId || userId == Guid.Empty)
            {
                return Result.Failure<OrderTrackingResponse>(
                    Error.New("OrderTracking.Unauthorized", "The access token does not identify a user."));
            }

            var order = await _unitOfWork.Repository<Order>()
                .Query()
                .AsNoTracking()
                .Include(o => o.AddressSnapshot)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
            {
                return Result.Failure<OrderTrackingResponse>(
                    Error.New("OrderTracking.NotFound", "Order was not found."));
            }

            // Tracking exposes a live position, so it is restricted to the customer who
            // placed the order rather than to any authenticated customer.
            if (order.UserId != userId)
            {
                return Result.Failure<OrderTrackingResponse>(
                    Error.New("OrderTracking.Forbidden", "You cannot track an order you did not place."));
            }

            var history = await LoadStatusHistoryAsync(order.Id, cancellationToken);
            var location = await ResolveLastKnownLocationAsync(order, cancellationToken);

            return Result.Success(new OrderTrackingResponse(
                order.Id,
                order.OrderNumber,
                order.Status,
                order.Status.IsActiveDelivery(),
                BuildTimeline(order.Status, history),
                BuildDriver(order),
                location,
                BuildDestination(order.AddressSnapshot)));
        }

        // Earliest entry per status: a status can only be entered once on the way forward,
        // but taking the first keeps the timeline honest if a row is ever duplicated.
        private async Task<IReadOnlyDictionary<OrderStatusEnum, DateTime>> LoadStatusHistoryAsync(
            Guid orderId, CancellationToken cancellationToken)
        {
            var entries = await _unitOfWork.Repository<OrderStatusHistory>()
                .Query()
                .AsNoTracking()
                .Where(h => h.OrderId == orderId)
                .Select(h => new { h.Status, h.OccurredAt })
                .ToListAsync(cancellationToken);

            return entries
                .GroupBy(h => h.Status)
                .ToDictionary(g => g.Key, g => g.Min(h => h.OccurredAt));
        }

        // Redis holds the position the driver last reported; the DriverLocations row is the
        // record of truth and covers a cold or evicted cache.
        private async Task<OrderTrackingLocationDto?> ResolveLastKnownLocationAsync(
            Order order, CancellationToken cancellationToken)
        {
            // Before pickup nobody is carrying the order, and afterwards the position is
            // history rather than a live fix - either way there is no marker to move.
            if (!order.Status.IsLiveDelivery())
                return null;

            var cached = await _locationCache.GetAsync(order.Id, cancellationToken);

            if (cached is not null)
                return ToLocationDto(cached.Lat, cached.Lng, cached.RecordedAt);

            var persisted = await _unitOfWork.Repository<DriverLocation>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.OrderId == order.Id, cancellationToken);

            return persisted is null
                ? null
                : ToLocationDto(persisted.Lat, persisted.Lng, persisted.RecordedAt);
        }

        private OrderTrackingLocationDto ToLocationDto(double lat, double lng, DateTime recordedAt)
        {
            // Clamped at zero so a ping recorded a fraction of a second ahead of this read
            // does not surface as a negative age.
            var age = DateTime.UtcNow - recordedAt;
            var ageSeconds = (int)Math.Max(0, Math.Round(age.TotalSeconds));

            return new OrderTrackingLocationDto(
                lat,
                lng,
                recordedAt,
                ageSeconds,
                ageSeconds > _settings.StalenessThresholdSeconds);
        }

        // Completion comes from OrderStatusHistory rather than from the current status
        // alone. It matters most for a cancelled order: the current status says nothing
        // about how far the order actually got, but the history does.
        private static IReadOnlyList<OrderTrackingStageDto> BuildTimeline(
            OrderStatusEnum status,
            IReadOnlyDictionary<OrderStatusEnum, DateTime> history)
        {
            var stages = OrderStatusExtensions.TimelineStages;

            // AwaitingDeliveryConfirmation is not a timeline stage of its own - to the
            // customer it is still "Out for Delivery" (ToDisplayString folds it the same
            // way), so it leaves the marker on the OutForDelivery stage until the driver
            // confirms completion and the order becomes Delivered.
            var timelineStatus = status == OrderStatusEnum.AwaitingDeliveryConfirmation
                ? OrderStatusEnum.OutForDelivery
                : status;
            var currentIndex = Array.IndexOf(stages, timelineStatus);

            var timeline = stages
                .Select((stage, index) => new OrderTrackingStageDto(
                    stage,
                    // Statuses only advance, so anything before the current stage happened
                    // even if it predates this order having any history recorded.
                    IsCompleted: index != currentIndex
                                 && (currentIndex >= 0 ? index < currentIndex : history.ContainsKey(stage)),
                    IsCurrent: index == currentIndex,
                    OccurredAt: history.TryGetValue(stage, out var occurredAt) ? occurredAt : null))
                .ToList();

            // Cancelled is not one of the forward stages, so it is appended as the terminal
            // one rather than replacing whichever stage the order had reached.
            if (status == OrderStatusEnum.Cancelled)
            {
                timeline.Add(new OrderTrackingStageDto(
                    OrderStatusEnum.Cancelled,
                    IsCompleted: true,
                    IsCurrent: true,
                    OccurredAt: history.TryGetValue(OrderStatusEnum.Cancelled, out var cancelledAt)
                        ? cancelledAt
                        : null));
            }

            return timeline;
        }

        // Null until a driver is assigned, which is what tells the client to render the
        // "waiting for a driver" state instead of an empty driver card.
        private static OrderTrackingDriverDto? BuildDriver(Order order) =>
            order.DriverId is not { } driverId
                ? null
                : new OrderTrackingDriverDto(
                    driverId,
                    order.DriverName,
                    order.DriverPhone,
                    order.DriverImageUrl,
                    order.DriverAssignedAt);

        private static OrderTrackingDestinationDto? BuildDestination(OrderAddressSnapshot? snapshot) =>
            snapshot is null
                ? null
                : new OrderTrackingDestinationDto(
                    snapshot.Lat,
                    snapshot.Lng,
                    snapshot.RecipientName,
                    snapshot.AddressLine,
                    snapshot.City,
                    snapshot.Area);
    }
}
