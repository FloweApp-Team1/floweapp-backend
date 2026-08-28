using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace OrdersService.Infrastructure.Services
{
    public class DistributedCacheIdempotencyService : IIdempotencyService
    {
        private readonly IDistributedCache _cache;
        private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

        public DistributedCacheIdempotencyService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<TResponse?> GetCachedResponseAsync<TResponse>(
            Guid userId, string idempotencyKey, CancellationToken cancellationToken) where TResponse : class
        {
            var raw = await _cache.GetStringAsync(BuildKey(userId, idempotencyKey), cancellationToken);
            return raw is null ? null : JsonSerializer.Deserialize<TResponse>(raw);
        }

        public async Task StoreResponseAsync<TResponse>(
            Guid userId, string idempotencyKey, TResponse response, CancellationToken cancellationToken)
            where TResponse : class
        {
            var json = JsonSerializer.Serialize(response);

            await _cache.SetStringAsync(
                BuildKey(userId, idempotencyKey),
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl },
                cancellationToken);
        }

        private static string BuildKey(Guid userId, string idempotencyKey) =>
            $"idempotency:place-order:{userId}:{idempotencyKey}";
    }
}
