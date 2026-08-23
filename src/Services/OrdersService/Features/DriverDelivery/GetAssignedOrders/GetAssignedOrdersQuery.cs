using MediatR;
using OrdersService.Domain.Enums;
using Shared.Requests;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrders
{
    public record GetAssignedOrdersQuery(PaginationRequest Request) : IRequest<Result<GetAssignedOrdersResponse>>;

    public record GetAssignedOrdersResponse(
        IReadOnlyList<AssignedOrderDto> Orders,
        int TotalCount);

    public record AssignedOrderDto(
        Guid Id,
        string OrderNumber,
        OrderStatusEnum Status,
        string StatusDisplay,
        DateTime PlacedAt,
        DateTime AssignedAt,
        int ItemCount,
        decimal Total,
        bool IsGift,
        AssignedOrderDestinationDto? Destination);

    public record AssignedOrderDestinationDto(
        string RecipientName,
        string RecipientPhone,
        string AddressLine,
        string City,
        string Area,
        double? Lat,
        double? Lng);
}
