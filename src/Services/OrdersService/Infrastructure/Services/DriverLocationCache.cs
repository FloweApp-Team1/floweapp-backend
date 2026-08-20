using Microsoft.Extensions.Options;
using OrdersService.Features.DriverDelivery.CacheDto;
using OrdersService.Infrastructure.Settings;
using Shared.Contracts;

namespace OrdersService.Infrastructure.Services
{
    // Redis-backed read path for the tracking endpoint. The SQL DriverLocations row stays the
    // record of truth; this exists so a customer polling every few seconds does not turn into
    // a database read per poll per order.
    public sealed class DriverLocationCache : IDriverLocationCache
    {
        private readonly IRedisCacheService _cache;
        private readonly ILogger<DriverLocationCache> _logger;
        private readonly TimeSpan _ttl;

        public DriverLocationCache(
            IRedisCacheService cache,
            IOptions<DeliveryTrackingSettings> settings,
            ILogger<DriverLocationCache> logger)
        {
            _cache = cache;
            _logger = logger;
            _ttl = TimeSpan.FromMinutes(settings.Value.CacheTtlMinutes);
        }

        private static string KeyFor(Guid orderId) => $"orders:tracking:driver-location:{orderId}";

        // Cache faults must not fail tracking: the caller falls back to the DriverLocations
        // row, which is always written first.
        public async Task<DriverLocationCacheDto?> GetAsync(
            Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cache.GetAsync<DriverLocationCacheDto>(KeyFor(orderId));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read the cached driver location for order {OrderId}.", orderId);
                return null;
            }
        }

        public async Task SetAsync(
            DriverLocationCacheDto location, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.SetAsync(KeyFor(location.OrderId), location, _ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not cache the driver location for order {OrderId}.", location.OrderId);
            }
        }

        public async Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(KeyFor(orderId));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not evict the cached driver location for order {OrderId}.", orderId);
            }
        }
    }
}
