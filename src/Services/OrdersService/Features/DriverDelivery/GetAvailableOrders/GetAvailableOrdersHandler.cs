using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.GetAvailableOrders
{ 
        public sealed class GetAvailableOrdersHandler
            : IRequestHandler<GetAvailableOrdersQuery, Result<GetAvailableOrdersResponse>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ICurrentUserService _currentUser;

            public GetAvailableOrdersHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            {
                _unitOfWork = unitOfWork;
                _currentUser = currentUser;
            }

            public async Task<Result<GetAvailableOrdersResponse>> Handle(
                GetAvailableOrdersQuery request, CancellationToken cancellationToken)
            {
            if (_currentUser.UserId is not { } driverId || driverId == Guid.Empty)
            {
                return Result.Failure<GetAvailableOrdersResponse>(
                    Error.New("AvailableOrders.Unauthorized", "The access token does not identify a driver."));
            }

            var query = _unitOfWork.Repository<Order>()
                    .Query()
                    .AsNoTracking()
                    .Where(o => o.Status == OrderStatusEnum.Preparing && o.DriverId == null);

                var totalCount = await query.CountAsync(cancellationToken);

                var orders = await query
                    .OrderBy(o => o.CreatedAt)
                    .Skip(request.Request.Skip)
                    .Take(request.Request.PageSize)
                    .Select(o => new AvailableOrderDto(
                        o.Id,
                        o.AddressSnapshot != null ? o.AddressSnapshot.RecipientName : string.Empty,
                        o.AddressSnapshot != null ? o.AddressSnapshot.AddressLine : string.Empty,
                        string.Empty,
                        string.Empty,
                        o.Total,
                        o.Status.ToDisplayString(),
                        o.CreatedAt))
                    .ToListAsync(cancellationToken);

                return Result.Success(new GetAvailableOrdersResponse(orders, totalCount));
            }
        }
}

