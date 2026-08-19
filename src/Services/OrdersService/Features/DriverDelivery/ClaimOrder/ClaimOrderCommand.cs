using MediatR;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.ClaimOrder
{
    // No body: the driver is the caller, and the order is in the route. Claiming is
    // first-come-first-served, so there is nothing left to specify.
    public record ClaimOrderCommand(Guid OrderId) : IRequest<Result<ClaimOrderResponse>>;
}
