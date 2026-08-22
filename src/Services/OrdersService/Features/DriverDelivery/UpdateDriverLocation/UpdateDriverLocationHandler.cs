using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Features.DriverDelivery.CacheDto;
using OrdersService.Features.DriverDelivery.Common;
using OrdersService.Infrastructure.Services;
using OrdersService.Infrastructure.Settings;
using Shared.Events.DriverDelivery;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.UpdateDriverLocation
{
    // SCRUM-152: persists a driver ping against every order that driver is currently
    // carrying, keeps the tracking read path warm in Redis, and raises the integration event
    // NotificationService turns into a silent push - but only when the fix moved far enough
    // or long enough to be worth waking the customer's device.
    public class UpdateDriverLocationHandler
        : IRequestHandler<UpdateDriverLocationCommand, Result<UpdateDriverLocationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDriverLocationCache _locationCache;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly ILogger<UpdateDriverLocationHandler> _logger;
        private readonly DeliveryTrackingSettings _settings;

        public UpdateDriverLocationHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDriverLocationCache locationCache,
            IIntegrationEventPublisher eventPublisher,
            IOptions<DeliveryTrackingSettings> settings,
            ILogger<UpdateDriverLocationHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _locationCache = locationCache;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<Result<UpdateDriverLocationResponse>> Handle(
            UpdateDriverLocationCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } driverId || driverId == Guid.Empty)
            {
                return Result.Failure<UpdateDriverLocationResponse>(
                    Error.New("DriverLocation.Unauthorized", "The access token does not identify a driver."));
            }

            var now = DateTime.UtcNow;

            // A device clock running ahead would make every later fix look stale to the
            // customer, so a future timestamp is pulled back to server time.
            var recordedAt = request.RecordedAt is { } clientTime && clientTime <= now
                ? clientTime
                : now;

            var activeOrders = await _unitOfWork.Repository<Order>()
                .GetAll(o => o.DriverId == driverId
                             && (o.Status == OrderStatusEnum.PickedUp
                                 || o.Status == OrderStatusEnum.OutForDelivery))
                .ToListAsync(cancellationToken);

            if (activeOrders.Count == 0)
            {
                return Result.Failure<UpdateDriverLocationResponse>(
                    Error.New("DriverLocation.Conflict",
                        "You have no order out for delivery, so there is nothing to report a location against."));
            }

            var orderIds = activeOrders.Select(o => o.Id).ToList();
            var locationRepository = _unitOfWork.Repository<DriverLocation>();

            var existingLocations = await locationRepository
                .GetAll(l => orderIds.Contains(l.OrderId))
                .ToListAsync(cancellationToken);

            var broadcasts = new List<DriverLocationUpdatedEvent>();
            var updatedOrderIds = new List<Guid>();

            foreach (var order in activeOrders)
            {
                var location = existingLocations.FirstOrDefault(l => l.OrderId == order.Id);

                // Pings can arrive out of order after a reconnect. An older fix must not
                // overwrite a newer one, or the marker would jump backwards.
                if (location is not null && location.RecordedAt > recordedAt)
                    continue;

                var shouldBroadcast = IsMeaningfulUpdate(location, request.Lat, request.Lng, now);

                if (location is null)
                {
                    location = new DriverLocation
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        DriverId = driverId,
                        CreatedAt = now
                    };

                    await locationRepository.AddAsync(location, cancellationToken);
                    existingLocations.Add(location);
                }

                location.DriverId = driverId;
                location.Lat = request.Lat;
                location.Lng = request.Lng;
                location.RecordedAt = recordedAt;
                location.UpdatedAt = now;
                location.LastChangedBy = driverId;

                if (shouldBroadcast)
                {
                    location.LastBroadcastAt = now;

                    broadcasts.Add(new DriverLocationUpdatedEvent(
                        order.Id,
                        order.OrderNumber,
                        order.UserId,
                        driverId,
                        request.Lat,
                        request.Lng,
                        recordedAt));
                }

                updatedOrderIds.Add(order.Id);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Cached after the write, so a reader can never see a position that failed to persist.
            foreach (var orderId in updatedOrderIds)
            {
                await _locationCache.SetAsync(
                    new DriverLocationCacheDto
                    {
                        OrderId = orderId,
                        DriverId = driverId,
                        Lat = request.Lat,
                        Lng = request.Lng,
                        RecordedAt = recordedAt
                    },
                    cancellationToken);
            }

            await BroadcastAsync(broadcasts, cancellationToken);

            return Result.Success(new UpdateDriverLocationResponse(
                recordedAt,
                updatedOrderIds,
                broadcasts.Count > 0));
        }

        // The ping is already persisted by this point, and another one follows within
        // seconds, so a broker outage costs the customer one map update rather than costing
        // the driver a failed request.
        private async Task BroadcastAsync(
            IReadOnlyList<DriverLocationUpdatedEvent> events, CancellationToken cancellationToken)
        {
            foreach (var locationEvent in events)
            {
                try
                {
                    await _eventPublisher.PublishAsync(locationEvent, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex,
                        "Could not publish the driver location update for order {OrderId}; the position was still saved.",
                        locationEvent.OrderId);
                }
            }
        }

        // The interval is measured against LastBroadcastAt, never RecordedAt: RecordedAt is
        // overwritten by every ping, so a driver pinging every two seconds would keep
        // resetting it and could go a whole delivery without the customer's map ever moving.
        // LastBroadcastAt only moves when a push actually went out, which is the thing the
        // throttle is rate-limiting.
        private bool IsMeaningfulUpdate(DriverLocation? previous, double lat, double lng, DateTime now)
        {
            if (previous is null)
                return true;

            // A row that has never been broadcast leaves the customer with no marker at all,
            // so the distance test would be measuring against something they cannot see.
            if (previous.LastBroadcastAt is not { } lastBroadcastAt)
                return true;

            // Doubles as a keep-alive: a driver stopped at a light still moves less than the
            // distance threshold, and going silent would tip the position into "stale".
            if (now - lastBroadcastAt >= TimeSpan.FromSeconds(_settings.MinimumBroadcastIntervalSeconds))
                return true;

            var moved = GeoCalculator.DistanceInMeters(previous.Lat, previous.Lng, lat, lng);

            return moved >= _settings.MinimumBroadcastDistanceMeters;
        }
    }
}
