using MediatR;
using Shared.Results;

namespace OrdersService.Features.Tracking.GetOrderTracking
{
    public record GetOrderTrackingQuery(Guid OrderId) : IRequest<Result<OrderTrackingResponse>>;
}
