using MediatR;
using Shared.Results;

namespace OrdersService.Features.Checkout.PlaceCodOrder
{
    public record PlaceCodOrderCommand(
        Guid StoreId,
        Guid? AddressId,
        bool IsGift,
        string? GiftRecipientName,
        string? GiftRecipientPhone,
        string? GiftRecipientAddress,
        List<PlaceCodOrderItem> Items) : IRequest<Result<PlaceCodOrderResponse>>;

    public record PlaceCodOrderItem(Guid ProductId, int Quantity);

    public record PlaceCodOrderResponse(Guid OrderId);
}
