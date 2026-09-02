using MediatR;
using OrdersService.Domain.Enums;
using Shared.Results;

namespace OrdersService.Features.Orders.AdminUpdateOrderStatus
{
    public record AdminUpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatusEnum Status,
    string? Note = null
) : IRequest<Result<AdminUpdateOrderStatusResponse>>;
}
