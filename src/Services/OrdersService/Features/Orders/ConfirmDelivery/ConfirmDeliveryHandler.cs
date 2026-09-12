using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;
using Shared.Events.OrderEvents;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.Orders.ConfirmDelivery;

public sealed class ConfirmDeliveryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IOrderStatusHistoryWriter historyWriter,
    IIntegrationEventPublisher eventPublisher,
    IDriverLocationCache locationCache,
    ILogger<ConfirmDeliveryHandler> logger)
    : IRequestHandler<ConfirmDeliveryCommand, Result<ConfirmDeliveryResponse>>
{
    public async Task<Result<ConfirmDeliveryResponse>> Handle(
        ConfirmDeliveryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } customerId || customerId == Guid.Empty)
        {
            return Result.Failure<ConfirmDeliveryResponse>(
                Error.New("ConfirmDelivery.Unauthorized", "Customer identity was not found."));
        }

        // Scope the read to the customer. Somebody else's order is indistinguishable from
        // a missing order, so order identifiers cannot be probed.
        var order = await unitOfWork.Repository<Order>()
            .FirstOrDefaultAsync(
                o => o.Id == request.OrderId && o.UserId == customerId,
                cancellationToken);

        if (order is null)
        {
            return Result.Failure<ConfirmDeliveryResponse>(
                Error.New("ConfirmDelivery.NotFound", "Order was not found."));
        }

        if (order.Status != OrderStatusEnum.AwaitingDeliveryConfirmation)
        {
            return Result.Failure<ConfirmDeliveryResponse>(
                Error.New("ConfirmDelivery.Conflict",
                    $"An order that is {order.Status} is not waiting for delivery confirmation."));
        }

        var confirmedAt = DateTime.UtcNow;
        var oldStatus = order.Status.ToString();

        order.Status = OrderStatusEnum.Delivered;
        order.UpdatedAt = confirmedAt;
        order.LastChangedBy = customerId;

        await historyWriter.RecordAsync(
            order,
            OrderStatusEnum.Delivered,
            confirmedAt,
            customerId,
            "Delivery confirmed by customer.",
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new OrderStatusUpdatedEvent(
            order.Id,
            order.UserId,
            oldStatus,
            order.Status.ToString(),
            confirmedAt), cancellationToken);

        try
        {
            await locationCache.DeleteAsync(order.Id, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex,
                "Order {OrderId} was confirmed delivered, but its cached driver location was not removed.",
                order.Id);
        }

        return Result.Success(new ConfirmDeliveryResponse(
            order.Id,
            order.Status,
            confirmedAt));
    }
}
