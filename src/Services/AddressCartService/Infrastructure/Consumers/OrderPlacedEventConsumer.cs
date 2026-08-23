using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.OrderEvents;

namespace AddressCartService.Infrastructure.Consumers
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly AddressCartDbContext _dbContext;
        private readonly ILogger<OrderPlacedEventConsumer> _logger;

        public OrderPlacedEventConsumer(
            AddressCartDbContext dbContext,
            ILogger<OrderPlacedEventConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("OrderPlacedEvent received for OrderId: {OrderId}, UserId: {UserId}", message.OrderId, message.UserId);

            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == message.UserId, context.CancellationToken);

            if (cart is null || cart.Items.Count == 0)
            {
                _logger.LogInformation("No items to clear for UserId: {UserId}", message.UserId);
                return;
            }

            _dbContext.CartItems.RemoveRange(cart.Items);
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Cleared {Count} cart items for UserId: {UserId} after Order {OrderId}", cart.Items.Count, message.UserId, message.OrderId);
        }
    }
}
