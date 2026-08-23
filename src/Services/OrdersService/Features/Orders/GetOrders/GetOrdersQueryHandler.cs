using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Interfaces;
using Shared.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrdersService.Features.Orders.GetOrders
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<GetOrdersResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetOrdersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<GetOrdersResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return Result<GetOrdersResponse>.Failure(Error.New("Unauthorized", "User is not authorized."));
            }

            var query = _unitOfWork.Repository<Order>().Query()
                .Where(o => o.UserId == userId);

            var totalCount = await query.CountAsync(cancellationToken);

            // Execute ordering, pagination and projection in the database
            var dbOrders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.Request.Page - 1) * request.Request.PageSize)
                .Take(request.Request.PageSize)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.CreatedAt,
                    ItemCount = o.Items.Count(),
                    FirstItemThumbnailUrl = o.Items.OrderBy(i => i.CreatedAt).Select(i => i.ProductImageUrl).FirstOrDefault(),
                    o.Status,
                    o.Total
                })
                .ToListAsync(cancellationToken);

            var orders = dbOrders.Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.CreatedAt,
                o.ItemCount,
                o.FirstItemThumbnailUrl,
                o.Status.ToDisplayString(),
                o.Total
            )).ToList();

            return Result<GetOrdersResponse>.Success(new GetOrdersResponse(orders, totalCount));
        }
    }
}
