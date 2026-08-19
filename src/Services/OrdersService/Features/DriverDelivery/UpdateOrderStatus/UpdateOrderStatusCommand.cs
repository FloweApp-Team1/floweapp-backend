using MediatR;
using OrdersService.Domain.Enums;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    public record UpdateOrderStatusCommand(
        Guid OrderId,
        OrderStatusEnum Status,
        string? Note = null) : IRequest<Result<UpdateOrderStatusResponse>>;
}
