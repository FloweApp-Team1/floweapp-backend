using OrdersService.Domain.Enums;

namespace OrdersService.Features.Orders.AdminUpdateOrderStatus
{
    public record AdminUpdateOrderStatusResponse(
    Guid OrderId,
    OrderStatusEnum Status,
    DateTime UpdatedAt
);
}
