using MediatR;
using Shared.Requests;
using Shared.Results;
using System;
using System.Collections.Generic;

namespace OrdersService.Features.Orders.GetOrders
{
    public record GetOrdersQuery(PaginationRequest Request) : IRequest<Result<GetOrdersResponse>>;

    public record GetOrdersResponse(
        IReadOnlyList<OrderSummaryDto> Orders,
        int TotalCount);

    public record OrderSummaryDto(
        Guid Id,
        string OrderNumber,
        DateTime CreatedAt,
        int ItemCount,
        string? FirstItemThumbnailUrl,
        string Status,
        decimal Total);
}
