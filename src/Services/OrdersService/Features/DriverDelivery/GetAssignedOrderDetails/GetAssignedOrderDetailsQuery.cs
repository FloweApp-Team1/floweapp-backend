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
        string AssignmentStatus,
        DateTime PlacedAt,
        DateTime? AssignedAt,
        decimal Subtotal,
        decimal DeliveryFee,
        decimal Total,
        string PaymentMethod,
        string PaymentMethodDisplay,
        string Currency,
        string PaymentStatus,
        bool IsGift,
        AssignedOrderPickupDto Pickup,
        AssignedOrderUserDto User,
        IReadOnlyList<AssignedOrderItemDto> Items,
        AssignedOrderDetailsDestinationDto? Destination);

    public record AssignedOrderUserDto(
        Guid Id,
        string Name,
        string Phone,
        string AddressLine,
        string City,
        string Area,
        double? Lat,
        double? Lng);

    public record AssignedOrderPickupDto(
        Guid StoreId,
        string Name,
        string? PhoneNumber,
        string? WhatsAppNumber,
        string? ImageUrl,
        string AddressLine,
        double? Lat,
        double? Lng);

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
