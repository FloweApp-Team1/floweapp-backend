using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.ClaimOrder
{
    // Assigns an unclaimed order to the driver making the request - the moment that turns
    // the customer's "waiting for a driver" state into a driver card, and the only thing
    // that ever sets Order.DriverId.
    public class ClaimOrderHandler
        : IRequestHandler<ClaimOrderCommand, Result<ClaimOrderResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDriverSnapshotService _driverSnapshots;
        private readonly ILogger<ClaimOrderHandler> _logger;

        public ClaimOrderHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDriverSnapshotService driverSnapshots,
            ILogger<ClaimOrderHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _driverSnapshots = driverSnapshots;
            _logger = logger;
        }

        public async Task<Result<ClaimOrderResponse>> Handle(
            ClaimOrderCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } driverId || driverId == Guid.Empty)
            {
                return Result.Failure<ClaimOrderResponse>(
                    Error.New("ClaimOrder.Unauthorized", "The access token does not identify a driver."));
            }

            var repository = _unitOfWork.Repository<Order>();
            var assignedAt = DateTime.UtcNow;

            var claimed = await repository
                .Query()
                .Where(o => o.Id == request.OrderId
                            && o.DriverId == null
                            && (o.Status == OrderStatusEnum.Placed
                                || o.Status == OrderStatusEnum.Preparing))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.DriverId, driverId)
                    .SetProperty(o => o.DriverAssignedAt, assignedAt)
                    .SetProperty(o => o.UpdatedAt, assignedAt)
                    .SetProperty(o => o.LastChangedBy, driverId),
                    cancellationToken);

            if (claimed == 0)
                return await ExplainFailureAsync(request.OrderId, driverId, cancellationToken);

            // Re-read rather than reusing anything from before the update: ExecuteUpdate
            // bypasses the change tracker, so this is the first point the in-memory order
            // and the row actually agree.
            var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);

            if (order is null)
            {
                return Result.Failure<ClaimOrderResponse>(
                    Error.New("ClaimOrder.NotFound", "Order was not found."));
            }

            await FillDriverSnapshotAsync(order, cancellationToken);

            return Result.Success(new ClaimOrderResponse(
                order.Id,
                order.OrderNumber,
                order.Status,
                assignedAt));
        }

        // This is the assignment moment, so it is where the driver's name, phone and photo
        // are copied from IdentityService - the customer should see a complete driver card
        // the first time they open tracking, not on some later read.
        private async Task FillDriverSnapshotAsync(Order order, CancellationToken cancellationToken)
        {
            try
            {
                await _driverSnapshots.EnsureSnapshotAsync(order, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Order {OrderId} was claimed, but the driver snapshot could not be filled.",
                    order.Id);
            }
        }

        private async Task<Result<ClaimOrderResponse>> ExplainFailureAsync(
            Guid orderId, Guid driverId, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Repository<Order>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order is null)
            {
                return Result.Failure<ClaimOrderResponse>(
                    Error.New("ClaimOrder.NotFound", "Order was not found."));
            }

            if (order.DriverId == driverId)
            {
                return Result.Failure<ClaimOrderResponse>(
                    Error.New("ClaimOrder.Conflict", "You have already claimed this order."));
            }

            if (order.DriverId is not null)
            {
                return Result.Failure<ClaimOrderResponse>(
                    Error.New("ClaimOrder.Conflict", "Another driver has already claimed this order."));
            }

            if (!order.Status.IsClaimable())
            {
                return Result.Failure<ClaimOrderResponse>(
                    Error.New("ClaimOrder.Conflict",
                        $"An order that is {order.Status} is no longer available to claim."));
            }

            return Result.Failure<ClaimOrderResponse>(
                Error.New("ClaimOrder.Conflict", "This order was not available a moment ago. Please try again."));
        }
    }
}
