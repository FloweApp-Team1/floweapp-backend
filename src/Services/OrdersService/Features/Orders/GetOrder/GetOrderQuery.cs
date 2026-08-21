using MediatR;
using Shared.Results;

namespace OrdersService.Features.Orders.GetOrder
{
    public record GetOrderQuery(Guid OrderId) : IRequest<Result<GetOrderResponse>>;

    public record GetOrderResponse(
        Guid Id, 
        string OrderNumber, 
        string Status, 
        string PaymentMethod, 
        string PaymentStatus, 
        decimal Subtotal, 
        decimal DeliveryFee, 
        decimal Total, 
        DateTime CreatedAt);
}
