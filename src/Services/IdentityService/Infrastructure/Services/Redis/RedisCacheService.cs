using IdentityService.Common.Contracts;
using StackExchange.Redis;
using System.Text.Json;

namespace IdentityService.Infrastructure.Services.Redis
{
    public sealed class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        public RedisCacheService(IConnectionMultiplexer multiplexer)
        {
            _database = multiplexer.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);

            await _database.StringSetAsync(key, json, expiry);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }

        public Task RemoveAsync(string key)
        {
            return _database.KeyDeleteAsync(key);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return _database.KeyExistsAsync(key);
        }
    }
}
