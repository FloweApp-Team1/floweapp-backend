using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Services
{
    public interface IDriverSnapshotService
    {
        Task<bool> EnsureSnapshotAsync(Order order, CancellationToken cancellationToken = default);
    }
}
