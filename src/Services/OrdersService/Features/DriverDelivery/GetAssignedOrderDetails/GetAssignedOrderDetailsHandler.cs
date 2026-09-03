using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrderDetails
{
    public class GetAssignedOrderDetailsHandler
        : IRequestHandler<GetAssignedOrderDetailsQuery, Result<GetAssignedOrderDetailsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetAssignedOrderDetailsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<GetAssignedOrderDetailsResponse>> Handle(
            GetAssignedOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } driverId || driverId == Guid.Empty)
            {
                return Result.Failure<GetAssignedOrderDetailsResponse>(
                    Error.New("AssignedOrderDetails.Unauthorized",
                        "The access token does not identify a driver."));
            }

            // Scoped to this driver's own orders in the query, so an order assigned to
            // someone else falls out here and is reported as 404 below - the contract
            // requires it to be indistinguishable from an order that does not exist, so a
            // driver cannot probe which order IDs are real.
            var order = await _unitOfWork.Repository<Order>()
                .Query()
                .AsNoTracking()
                .Where(o => o.Id == request.OrderId && o.DriverId == driverId)
                .Select(o => new
                {
                    o.Id,
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
                    Error.New("AssignedOrderDetails.NotFound", "Order was not found."));
            }

            return Result.Success(new GetAssignedOrderDetailsResponse(
                order.Id,
                order.OrderNumber,
                order.Status,
                order.Status.ToDisplayString(),
                order.CreatedAt,
                order.DriverAssignedAt,
                order.Subtotal,
                order.DeliveryFee,
                order.Total,
                order.PaymentMethod.ToString(),
                order.PaymentStatus.ToString(),
                order.IsGift,
                order.Items,
                order.Destination));
        }
    }
}
