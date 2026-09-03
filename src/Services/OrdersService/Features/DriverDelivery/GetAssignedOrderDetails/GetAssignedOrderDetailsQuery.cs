using MediatR;
using OrdersService.Domain.Enums;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrderDetails
{
    public record GetAssignedOrderDetailsQuery(Guid OrderId)
        : IRequest<Result<GetAssignedOrderDetailsResponse>>;


    public record GetAssignedOrderDetailsResponse(
        Guid Id,
        string OrderNumber,
        OrderStatusEnum Status,
        string StatusDisplay,
        DateTime PlacedAt,
        DateTime? AssignedAt,
        decimal Subtotal,
        decimal DeliveryFee,
        decimal Total,
        string PaymentMethod,
        string PaymentStatus,
        bool IsGift,
        IReadOnlyList<AssignedOrderItemDto> Items,
        AssignedOrderDetailsDestinationDto? Destination);

    public record AssignedOrderItemDto(
        Guid ProductId,
        string ProductName,
        string? ProductImageUrl,
        decimal UnitPrice,
        int Quantity);

    public record AssignedOrderDetailsDestinationDto(
        string RecipientName,
        string RecipientPhone,
        string AddressLine,
        string City,
        string Area,
        double? Lat,
        double? Lng);
}
