using MediatR;
using OrdersService.Domain.Enums;
using Shared.Results;

namespace OrdersService.Features.Orders.ConfirmDelivery;

public sealed record ConfirmDeliveryCommand(Guid OrderId)
    : IRequest<Result<ConfirmDeliveryResponse>>;

public sealed record ConfirmDeliveryResponse(
    Guid OrderId,
    OrderStatusEnum Status,
    DateTime ConfirmedAt);
