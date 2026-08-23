using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Cart.AddCartItem
{
    public sealed record AddCartItemRequest(Guid ProductId, int Quantity);

    public sealed record AddCartItemCommand(Guid ProductId, int Quantity)
        : IRequest<Result<AddCartItemResponse>>;

    public sealed record AddCartItemResponse(
        Guid CartId,
        Guid ItemId,
        Guid ProductId,
        int Quantity,
        decimal PriceAtAdd);
}
