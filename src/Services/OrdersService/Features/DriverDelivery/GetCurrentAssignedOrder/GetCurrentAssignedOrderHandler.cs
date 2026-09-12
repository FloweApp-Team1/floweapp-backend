using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Features.DriverDelivery.GetAssignedOrderDetails;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetCurrentAssignedOrder;

public sealed class GetCurrentAssignedOrderHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GetCurrentAssignedOrderQuery, Result<GetAssignedOrderDetailsResponse>>
{
    public async Task<Result<GetAssignedOrderDetailsResponse>> Handle(
        GetCurrentAssignedOrderQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } driverId || driverId == Guid.Empty)
        {
            return Result.Failure<GetAssignedOrderDetailsResponse>(
                Error.New("CurrentAssignedOrder.Unauthorized",
                    "The access token does not identify a driver."));
        }

        var order = await unitOfWork.Repository<Order>()
            .Query()
            .AsNoTracking()
            .Where(o => o.DriverId == driverId
                        && o.Status != OrderStatusEnum.Delivered
                        && o.Status != OrderStatusEnum.Cancelled)
            .OrderByDescending(o => o.DriverAssignedAt)
            .Select(o => new
            {
                o.Id,
                o.UserId,
                o.StoreId,
                o.StoreName,
                o.StorePhoneNumber,
                o.StoreWhatsAppNumber,
                o.StoreImageUrl,
                o.StoreAddressLine,
                o.StoreLat,
                o.StoreLng,
                o.OrderNumber,
                o.Status,
                o.CreatedAt,
                o.DriverAssignedAt,
                o.Subtotal,
                o.DeliveryFee,
                o.Total,
                o.PaymentMethod,
                o.PaymentStatus,
                o.IsGift,
                Items = o.Items.Select(i => new AssignedOrderItemDto(
                    i.ProductId,
                    i.ProductName,
                    i.ProductImageUrl,
                    i.UnitPrice,
                    i.Quantity)).ToList(),
                Destination = o.AddressSnapshot == null ? null : new AssignedOrderDetailsDestinationDto(
                    o.AddressSnapshot.RecipientName,
                    o.AddressSnapshot.RecipientPhone,
                    o.AddressSnapshot.AddressLine,
                    o.AddressSnapshot.City,
                    o.AddressSnapshot.Area,
                    o.AddressSnapshot.Lat,
                    o.AddressSnapshot.Lng)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<GetAssignedOrderDetailsResponse>(
                Error.New("CurrentAssignedOrder.NotFound",
                    "The driver has no active assigned order."));
        }

        return Result.Success(new GetAssignedOrderDetailsResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Status.ToDisplayString(),
            "Accepted",
            order.CreatedAt,
            order.DriverAssignedAt,
            order.Subtotal,
            order.DeliveryFee,
            order.Total,
            order.PaymentMethod.ToString(),
            order.PaymentMethod == PaymentMethodEnum.Cod ? "Cash on delivery" : "Card",
            "EGP",
            order.PaymentStatus.ToString(),
            order.IsGift,
            new AssignedOrderPickupDto(
                order.StoreId,
                order.StoreName ?? string.Empty,
                order.StorePhoneNumber,
                order.StoreWhatsAppNumber,
                order.StoreImageUrl,
                order.StoreAddressLine ?? string.Empty,
                order.StoreLat,
                order.StoreLng),
            new AssignedOrderUserDto(
                order.UserId,
                order.Destination?.RecipientName ?? string.Empty,
                order.Destination?.RecipientPhone ?? string.Empty,
                order.Destination?.AddressLine ?? string.Empty,
                order.Destination?.City ?? string.Empty,
                order.Destination?.Area ?? string.Empty,
                order.Destination?.Lat,
                order.Destination?.Lng),
            order.Items,
            order.Destination));
    }
}
