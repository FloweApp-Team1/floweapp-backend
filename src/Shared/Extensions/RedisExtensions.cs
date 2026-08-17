using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;
using Shared.Services.Redis;
using StackExchange.Redis;
using System;

namespace Shared.Extensions
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddSharedRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["Redis:ConnectionString"] 
                ?? throw new InvalidOperationException("Required configuration 'Redis:ConnectionString' is not set.");

            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddSingleton<IRedisCacheService, RedisCacheService>();

            return services;
        }
    }
}
