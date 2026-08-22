using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    // Advances an order through the delivery stages the driver controls. This is what moves
    // the customer's tracking timeline, so every transition is also written to
    // OrderStatusHistory - the timeline needs to say when a stage happened, and Order only
    // ever holds the latest status.
    public class UpdateOrderStatusHandler
        : IRequestHandler<UpdateOrderStatusCommand, Result<UpdateOrderStatusResponse>>
    {
        // The stages a driver may set. Placed is set at checkout and Preparing belongs to
        // the store, so neither is reachable from this driver-authenticated endpoint even
        // though both are valid order statuses.
        private static readonly OrderStatusEnum[] DriverControlledStatuses =
        [
            OrderStatusEnum.PickedUp,
            OrderStatusEnum.OutForDelivery,
            OrderStatusEnum.Delivered,
            OrderStatusEnum.Cancelled
        ];

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IOrderStatusHistoryWriter _historyWriter;
        private readonly IDriverLocationCache _locationCache;
        private readonly IDriverSnapshotService _driverSnapshots;
        private readonly ILogger<UpdateOrderStatusHandler> _logger;

        public UpdateOrderStatusHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IOrderStatusHistoryWriter historyWriter,
            IDriverLocationCache locationCache,
            IDriverSnapshotService driverSnapshots,
            ILogger<UpdateOrderStatusHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _historyWriter = historyWriter;
            _locationCache = locationCache;
            _driverSnapshots = driverSnapshots;
            _logger = logger;
        }

        public async Task<Result<UpdateOrderStatusResponse>> Handle(
            UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } driverId || driverId == Guid.Empty)
            {
                return Failure("OrderStatus.Unauthorized",
                    "The access token does not identify a driver.");
            }

            var order = await _unitOfWork.Repository<Order>()
                .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

            if (order is null)
                return Failure("OrderStatus.NotFound", "Order was not found.");

            if (Authorize(order, driverId, request.Status) is { } authorizationError)
                return Result.Failure<UpdateOrderStatusResponse>(authorizationError);

            if (ValidateTransition(order.Status, request.Status) is { } transitionError)
                return Result.Failure<UpdateOrderStatusResponse>(transitionError);

            var occurredAt = DateTime.UtcNow;

            order.Status = request.Status;
            order.UpdatedAt = occurredAt;
            order.LastChangedBy = driverId;

            await _historyWriter.RecordAsync(
                order, request.Status, occurredAt, driverId, request.Note, cancellationToken);

            // One SaveChanges is already one transaction, so the order row and its history
            // entry commit together or not at all. Wrapping it in an explicit Begin/Commit
            // would add nothing but a second ordering to get wrong.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await AfterCommitAsync(order, cancellationToken);

            return Result.Success(new UpdateOrderStatusResponse(
                order.Id, order.Status, occurredAt));
        }

        // Best-effort follow-up work. The status change is already durable, so nothing here
        // is allowed to turn a successful transition into a failed request - a driver who
        // has handed over the flowers must not be told the delivery did not register.
        private async Task AfterCommitAsync(Order order, CancellationToken cancellationToken)
        {
            try
            {
                if (!order.Status.IsLiveDelivery())
                {
                    // Nobody is carrying the order any more, and the tracking endpoint stops
                    // reporting a position for it. Dropping the key now means the last fix of
                    // a finished delivery is not left sitting in Redis until its TTL expires.
                    await _locationCache.DeleteAsync(order.Id, cancellationToken);
                }

                // Backfills the driver card for an order assigned by some path that did
                // not snapshot it. The driver is the caller here, so their token carries
                // the identity - the same source the claim endpoint fills it from.
                if (order.Status == OrderStatusEnum.PickedUp)
                    await _driverSnapshots.EnsureSnapshotAsync(order, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Order {OrderId} moved to {Status}, but the follow-up work did not complete.",
                    order.Id, order.Status);
            }
        }

        private static Error? Authorize(Order order, Guid driverId, OrderStatusEnum newStatus)
        {
            if (!DriverControlledStatuses.Contains(newStatus))
            {
                return Error.New("OrderStatus.Forbidden",
                    $"{newStatus} is not a status a driver can set.");
            }

            if (order.DriverId is not { } assignedDriverId)
            {
                return Error.New("OrderStatus.Conflict",
                    "A driver must be assigned to this order before its status can change.");
            }

            // Checked against the assigned driver rather than merely "is a driver", so an
            // approved driver cannot move somebody else's delivery along.
            if (assignedDriverId != driverId)
            {
                return Error.New("OrderStatus.Forbidden",
                    "You are not assigned to this order.");
            }

            return null;
        }

        // Statuses only ever move forward along the delivery path, or sideways to Cancelled
        // before the order is handed over.
        private static Error? ValidateTransition(OrderStatusEnum current, OrderStatusEnum next)
        {
            if (current == next)
                return Error.New("OrderStatus.Conflict", $"Order is already {next}.");

            var isValid = current switch
            {
                OrderStatusEnum.Placed => next is OrderStatusEnum.Preparing or OrderStatusEnum.Cancelled,
                OrderStatusEnum.Preparing => next is OrderStatusEnum.PickedUp or OrderStatusEnum.Cancelled,
                OrderStatusEnum.PickedUp => next is OrderStatusEnum.OutForDelivery or OrderStatusEnum.Cancelled,
                OrderStatusEnum.OutForDelivery => next is OrderStatusEnum.Delivered or OrderStatusEnum.Cancelled,

                // Delivered and Cancelled are terminal.
                _ => false
            };

            return isValid
                ? null
                : Error.New("OrderStatus.Conflict", $"An order cannot go from {current} to {next}.");
        }

        private static Result<UpdateOrderStatusResponse> Failure(string code, string message) =>
            Result.Failure<UpdateOrderStatusResponse>(Error.New(code, message));
    }
}
