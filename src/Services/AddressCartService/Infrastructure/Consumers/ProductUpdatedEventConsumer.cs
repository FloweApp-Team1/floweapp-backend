using MassTransit;
using Shared.Events.IntegrationEvents;

namespace AddressCartService.Infrastructure.Consumers
{
    public class ProductUpdatedEventConsumer : IConsumer<ProductUpdatedEvent>
    {
        private readonly ILogger<ProductUpdatedEventConsumer> _logger;

        public ProductUpdatedEventConsumer(ILogger<ProductUpdatedEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            _logger.LogInformation("ProductUpdatedEvent received for ProductId: {ProductId}. GetCart HTTP validation will reflect live product data.", context.Message.ProductId);
            return Task.CompletedTask;
        }
    }
}
