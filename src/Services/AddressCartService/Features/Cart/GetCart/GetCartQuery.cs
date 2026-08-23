using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Cart.GetCart
{
    public sealed record GetCartQuery(Guid? CartId = null) : IRequest<Result<GetCartResponse>>;

    public sealed record CartItemResponse(
        Guid ItemId,
        Guid ProductId,
        string ProductName,
        string? ProductImage,
        decimal UnitPrice,
        decimal PriceAtAdd,
        int Quantity,
        decimal LineSubtotal,
        int AvailableStock,
        bool IsAvailable,
        bool PriceChanged,
        bool StockChanged);

    public sealed record GetCartResponse(
        Guid CartId,
        IReadOnlyList<CartItemResponse> Items,
        int TotalQuantity,
        int LineCount,
        decimal Subtotal,
        decimal? DeliveryFee,
        decimal Total,
        bool HasChanges);
}
