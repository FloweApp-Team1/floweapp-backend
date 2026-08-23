using AddressCartService.Features.Cart.GetCart;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Cart.RemoveCartItem
{
    public sealed record RemoveCartItemCommand(Guid ItemId)
        : IRequest<Result<GetCartResponse>>;
}
