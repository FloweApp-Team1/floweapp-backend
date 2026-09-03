using MediatR;
using OrdersService.Domain.Enums;
using Shared.Requests;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrders
{
    // Status is the contract's optional history filter: absent means "every assigned order",
    // otherwise only orders currently in that status.
    public record GetAssignedOrdersQuery(
        PaginationRequest Request,
        OrderStatusEnum? Status = null) : IRequest<Result<GetAssignedOrdersResponse>>;

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
