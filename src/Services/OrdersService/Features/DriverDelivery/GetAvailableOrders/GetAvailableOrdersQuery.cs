using MediatR;
using Shared.Requests;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetAvailableOrders
{
    public sealed record GetAvailableOrdersQuery(PaginationRequest Request)
        : IRequest<Result<GetAvailableOrdersResponse>>;
}
