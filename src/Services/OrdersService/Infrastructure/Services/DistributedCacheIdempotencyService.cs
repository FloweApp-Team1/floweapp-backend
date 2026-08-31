using StackExchange.Redis;
using System.Text.Json;
using static OrdersService.Infrastructure.Services.IIdempotencyService;

namespace OrdersService.Infrastructure.Services
{
    public class DistributedCacheIdempotencyService : IIdempotencyService
    {
        private readonly IConnectionMultiplexer _redis;
        private static readonly TimeSpan ResultTtl = TimeSpan.FromHours(24);
        private static readonly TimeSpan ReservationTtl = TimeSpan.FromMinutes(2);

        private const string InProgressMarker = "__IN_PROGRESS__";

        public DistributedCacheIdempotencyService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<IdempotencyReservation<TResponse>> TryReserveAsync<TResponse>(
            Guid userId, string idempotencyKey, CancellationToken cancellationToken) where TResponse : class
        {
            var db = _redis.GetDatabase();
            var key = BuildKey(userId, idempotencyKey);

            var acquired = await db.StringSetAsync(key, InProgressMarker, ReservationTtl, When.NotExists);

            if (acquired)
                return new IdempotencyReservation<TResponse>(true, false, null);

            var existing = await db.StringGetAsync(key);

            if (existing.IsNullOrEmpty || existing == InProgressMarker)
                return new IdempotencyReservation<TResponse>(false, false, null);

            var cached = JsonSerializer.Deserialize<TResponse>(existing!);
            return new IdempotencyReservation<TResponse>(false, true, cached);
        }

        public async Task CompleteReservationAsync<TResponse>(
            Guid userId, string idempotencyKey, TResponse response, CancellationToken cancellationToken)
            where TResponse : class
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(response);
            await db.StringSetAsync(BuildKey(userId, idempotencyKey), json, ResultTtl);
        }

        public async Task ReleaseReservationAsync(
            Guid userId, string idempotencyKey, CancellationToken cancellationToken)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(BuildKey(userId, idempotencyKey));
        }

        private static string BuildKey(Guid userId, string idempotencyKey) =>
            $"idempotency:place-order:{userId}:{idempotencyKey}";
    }
}