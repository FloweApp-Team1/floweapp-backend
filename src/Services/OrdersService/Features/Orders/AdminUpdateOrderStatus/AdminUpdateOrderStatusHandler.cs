using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Features.Orders.AdminUpdateOrderStatus;
using OrdersService.Infrastructure.Services;
using Shared.Events.OrderEvents;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.AdminUpdateOrderStatus;

public class AdminUpdateOrderStatusHandler
    : IRequestHandler<AdminUpdateOrderStatusCommand, Result<AdminUpdateOrderStatusResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IOrderStatusHistoryWriter _historyWriter;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public AdminUpdateOrderStatusHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IOrderStatusHistoryWriter historyWriter,
        IIntegrationEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _historyWriter = historyWriter;
        _eventPublisher = eventPublisher;
    }


    public async Task<Result<AdminUpdateOrderStatusResponse>> Handle(
        AdminUpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        // Validate admin identity
        if (_currentUser.UserId is not { } adminId ||
            adminId == Guid.Empty)
        {
            return Result.Failure<AdminUpdateOrderStatusResponse>(
                Error.New(
                    "OrderStatus.Unauthorized",
                    "Admin identity was not found."));
        }

        // Fetch the order from the database

        var order = await _unitOfWork.Repository<Order>()
            .FirstOrDefaultAsync(
                x => x.Id == request.OrderId,
                cancellationToken);


        if (order is null)
        {
            return Result.Failure<AdminUpdateOrderStatusResponse>(
                Error.New(
                    "OrderStatus.NotFound",
                    "Order was not found."));
        }

        // Validate the current order status

        if (order.Status != OrderStatusEnum.Placed)
        {
            return Result.Failure<AdminUpdateOrderStatusResponse>(
                Error.New(
                    "OrderStatus.Conflict",
                    $"Order cannot move from {order.Status} to Preparing."));
        }

        // Update the order status to Preparing


        var occurredAt = DateTime.UtcNow;

        var oldStatus = order.Status.ToString();


        order.Status = OrderStatusEnum.Preparing;
        order.UpdatedAt = occurredAt;
        order.LastChangedBy = adminId;

        // Record the status change in the order history

        await _historyWriter.RecordAsync(
            order,
            OrderStatusEnum.Preparing,
            occurredAt,
            adminId,
            request.Note,
            cancellationToken);


        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish an event to notify other services about the status change
        await _eventPublisher.PublishAsync(
            new OrderStatusUpdatedEvent(
                order.Id,
                order.UserId,
                oldStatus,
                order.Status.ToString(),
                occurredAt),
            cancellationToken);


        return Result.Success(
            new AdminUpdateOrderStatusResponse(
                order.Id,
                order.Status,
                occurredAt));
    }
}