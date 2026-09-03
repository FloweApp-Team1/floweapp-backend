using MediatR;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    // Status is DriverStatusUpdate, not OrderStatusEnum: the set of statuses a driver may
    // request is a type constraint here, so the handler never has to defend against
    // Delivered or Cancelled arriving through this path.
    public record UpdateOrderStatusCommand(
        Guid OrderId,
        DriverStatusUpdate Status,
        string? Note = null) : IRequest<Result<UpdateOrderStatusResponse>>;
}
