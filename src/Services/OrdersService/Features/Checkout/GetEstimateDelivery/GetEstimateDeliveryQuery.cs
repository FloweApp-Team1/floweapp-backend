using MediatR;
using Shared.Results;

namespace OrdersService.Features.Checkout.GetEstimateDelivery
{
    public sealed record GetEstimateDeliveryQuery(Guid AddressId, Guid CartId)
      : IRequest<Result<EstimateDeliveryResponse>>;
}
