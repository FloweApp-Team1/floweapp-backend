using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace OrdersService.Features.Checkout.GetCheckoutDetails
{
    public sealed record GetCheckoutDetailsQuery(Guid CartId)
        : IRequest<Result<CheckoutDetailsResponse>>;
}

