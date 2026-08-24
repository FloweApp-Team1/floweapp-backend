using AddressCartService.Features.Cart;
using MassTransit;
using Shared.Contracts;
using Shared.Events.OrderEvents;

namespace AddressCartService.Infrastructure.Consumers
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IRedisCacheService _redisCache;
        private readonly ILogger<OrderPlacedEventConsumer> _logger;

        public OrderPlacedEventConsumer(
            IRedisCacheService redisCache,
            ILogger<OrderPlacedEventConsumer> logger)
        {
            _redisCache = redisCache;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("OrderPlacedEvent received for OrderId: {OrderId}, UserId: {UserId}", message.OrderId, message.UserId);

            var cacheKey = CartCacheKeys.Cart(message.UserId);
            
            // Just remove the key entirely. Redis Remove is naturally idempotent.
            await _redisCache.RemoveAsync(cacheKey);

            _logger.LogInformation("Cleared cart for UserId: {UserId} after Order {OrderId}", message.UserId, message.OrderId);
        }
    }
}
