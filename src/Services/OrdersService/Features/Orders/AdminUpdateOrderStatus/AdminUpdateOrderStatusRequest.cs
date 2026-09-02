using OrdersService.Domain.Enums;

namespace OrdersService.Features.Orders.AdminUpdateOrderStatus
{
    public record AdminUpdateOrderStatusRequest(
     OrderStatusEnum Status,
     string? Note
 );
}
