using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;

namespace OrdersService.Infrastructure.Services
{
    // Single writer for OrderStatusHistory, so every transition is recorded the same way
    // and the backfill rule lives in one place rather than in each feature that moves an
    // order forward.
    public interface IOrderStatusHistoryWriter
    {
        // Queues the history row on the unit of work without saving: the caller owns the
        // SaveChanges, so the status change and its history row land in one transaction and
        // an order can never advance without leaving a trace.
        Task RecordAsync(
            Order order,
            OrderStatusEnum status,
            DateTime occurredAt,
            Guid? changedBy,
            string? note = null,
            CancellationToken cancellationToken = default);
    }
}
