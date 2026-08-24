using AddressCartService.Features.Cart;
using MassTransit;
using Shared.Contracts;
using Shared.Events.OrderEvents;

namespace AddressCartService.Infrastructure.Consumers
{
    public class ClearCartOnOrderConfirmedConsumer : IConsumer<OrderConfirmedEvent>
    {
        private readonly IRedisCacheService _redisCache;
        private readonly ILogger<ClearCartOnOrderConfirmedConsumer> _logger;

        public ClearCartOnOrderConfirmedConsumer(
            IRedisCacheService redisCache,
            ILogger<ClearCartOnOrderConfirmedConsumer> logger)
        {
            _redisCache = redisCache;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("OrderConfirmedEvent received for OrderId: {OrderId}, UserId: {UserId}", message.OrderId, message.UserId);

            var cacheKey = CartCacheKeys.Cart(message.UserId);
            
            // Just remove the key entirely. Redis Remove is naturally idempotent.
            await _redisCache.RemoveAsync(cacheKey);

            _logger.LogInformation("Cleared cart for UserId: {UserId} after Order {OrderId} was confirmed.", message.UserId, message.OrderId);
        }
    }
}
