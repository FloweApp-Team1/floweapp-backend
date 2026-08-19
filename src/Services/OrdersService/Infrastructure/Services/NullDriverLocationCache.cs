using OrdersService.Features.DriverDelivery.CacheDto;

namespace OrdersService.Infrastructure.Services
{
    // Used when Redis is not configured. Tracking still works: GetOrderTrackingHandler falls
    // back to the DriverLocations row, which is the record of truth. Only the read-path cache
    // is lost, so this trades throughput for the service being able to start without Redis.
    public sealed class NullDriverLocationCache : IDriverLocationCache
    {
        public Task<DriverLocationCacheDto?> GetAsync(
            Guid orderId, CancellationToken cancellationToken = default)
            => Task.FromResult<DriverLocationCacheDto?>(null);

        public Task SetAsync(
            DriverLocationCacheDto location, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
