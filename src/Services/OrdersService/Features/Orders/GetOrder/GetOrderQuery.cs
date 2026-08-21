using MediatR;
using Shared.Results;

namespace OrdersService.Features.Orders.GetOrder
{
    public record GetOrderQuery(Guid OrderId) : IRequest<Result<GetOrderResponse>>;

    public record GetOrderResponse(
        Guid Id, 
        string OrderNumber, 
        string Status, 
        string PaymentMethod, 
        string PaymentStatus, 
        decimal Subtotal, 
        decimal DeliveryFee, 
        decimal Total, 
        DateTime CreatedAt,
        List<OrderItemDto> Items,
        OrderAddressDto? DeliveryAddress,
        bool IsGift,
        string? GiftRecipientName,
        string? GiftRecipientPhone,
        string? GiftRecipientAddress,
        bool IsLiveTrackingAvailable);

    public record OrderItemDto(
        Guid ProductId,
        string ProductName,
        string? ProductImageUrl,
        decimal UnitPrice,
        int Quantity);

    public record OrderAddressDto(
        string City,
        string Area,
        string AddressLine,
        string RecipientName,
        string RecipientPhone,
        double? Latitude,
        double? Longitude);
}
