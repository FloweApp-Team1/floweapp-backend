using MediatR;
using OrdersService.Features.DriverDelivery.GetAssignedOrderDetails;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetCurrentAssignedOrder;

public sealed record GetCurrentAssignedOrderQuery
    : IRequest<Result<GetAssignedOrderDetailsResponse>>;
