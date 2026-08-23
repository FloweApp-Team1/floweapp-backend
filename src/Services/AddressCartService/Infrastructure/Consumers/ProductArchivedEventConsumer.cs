using MassTransit;
using Shared.Events.IntegrationEvents;

namespace AddressCartService.Infrastructure.Consumers
{
    public class ProductArchivedEventConsumer : IConsumer<ProductArchivedEvent>
    {
        private readonly ILogger<ProductArchivedEventConsumer> _logger;

        public ProductArchivedEventConsumer(ILogger<ProductArchivedEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ProductArchivedEvent> context)
        {
            _logger.LogInformation("ProductArchivedEvent received for ProductId: {ProductId}. GetCart HTTP validation will flag this item as unavailable.", context.Message.ProductId);
            return Task.CompletedTask;
        }
    }
}
