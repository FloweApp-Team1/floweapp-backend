using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrders
{
    public class GetAssignedOrdersHandler
        : IRequestHandler<GetAssignedOrdersQuery, Result<GetAssignedOrdersResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetAssignedOrdersHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<GetAssignedOrdersResponse>> Handle(
            GetAssignedOrdersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } driverId || driverId == Guid.Empty)
            {
                return Result.Failure<GetAssignedOrdersResponse>(
                    Error.New("AssignedOrders.Unauthorized", "The access token does not identify a driver."));
            }

            var query = _unitOfWork.Repository<Order>()
                .Query()
                .AsNoTracking()
                .Where(o => o.DriverId == driverId);

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.DriverAssignedAt)
                .Skip(request.Request.Skip)
                .Take(request.Request.PageSize)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.Status,
                    o.CreatedAt,
                    AssignedAt = o.DriverAssignedAt!.Value,
                    ItemCount = o.Items.Count(),
                    o.Total,
                    o.IsGift,
                    Destination = o.AddressSnapshot == null ? null : new
                    {
                        o.AddressSnapshot.RecipientName,
                        o.AddressSnapshot.RecipientPhone,
                        o.AddressSnapshot.AddressLine,
                        o.AddressSnapshot.City,
                        o.AddressSnapshot.Area,
                        o.AddressSnapshot.Lat,
                        o.AddressSnapshot.Lng
                    }
                })
                .ToListAsync(cancellationToken);

            var dtos = orders
                .Select(o => new AssignedOrderDto(
                    o.Id,
                    o.OrderNumber,
                    o.Status,
                    o.Status.ToDisplayString(),
                    o.CreatedAt,
                    o.AssignedAt,
                    o.ItemCount,
                    o.Total,
                    o.IsGift,
                    o.Destination is null
                        ? null
                        : new AssignedOrderDestinationDto(
                            o.Destination.RecipientName,
                            o.Destination.RecipientPhone,
                            o.Destination.AddressLine,
                            o.Destination.City,
                            o.Destination.Area,
                            o.Destination.Lat,
                            o.Destination.Lng)))
                .ToList();

            return Result.Success(new GetAssignedOrdersResponse(dtos, totalCount));
        }
    }
}
