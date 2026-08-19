using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Interfaces;

namespace OrdersService.Infrastructure.Services
{
    public class OrderStatusHistoryWriter : IOrderStatusHistoryWriter
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderStatusHistoryWriter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task RecordAsync(
            Order order,
            OrderStatusEnum status,
            DateTime occurredAt,
            Guid? changedBy,
            string? note = null,
            CancellationToken cancellationToken = default)
        {
            var repository = _unitOfWork.Repository<OrderStatusHistory>();

            var hasHistory = await repository
                .Query()
                .AnyAsync(h => h.OrderId == order.Id, cancellationToken);

            // Orders placed before this table existed - and any placed by a checkout path
            // that does not write history yet - would otherwise show a timeline whose
            // earlier stages are complete but undated. The order's own CreatedAt is the
            // moment it was placed, so that first stage can be recovered exactly.
            if (!hasHistory)
            {
                await repository.AddAsync(
                    NewEntry(order, OrderStatusEnum.Placed, order.CreatedAt, changedBy: null, note: null),
                    cancellationToken);
            }

            await repository.AddAsync(
                NewEntry(order, status, occurredAt, changedBy, note),
                cancellationToken);
        }

        private static OrderStatusHistory NewEntry(
            Order order,
            OrderStatusEnum status,
            DateTime occurredAt,
            Guid? changedBy,
            string? note) => new()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = status,
                OccurredAt = occurredAt,
                ChangedBy = changedBy,
                Note = note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedBy = changedBy ?? Guid.Empty
            };
    }
}
