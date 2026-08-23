using AddressCartService.Features.Cart.GetCart;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Cart.UpdateCartItem
{
    public sealed record UpdateCartItemRequest(int Quantity);

    public sealed record UpdateCartItemCommand(Guid ItemId, int Quantity)
        : IRequest<Result<GetCartResponse>>;
}
