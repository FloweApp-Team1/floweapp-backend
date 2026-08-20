using MassTransit;
using Shared.Interfaces;

namespace OrdersService.Infrastructure.Messaging
{
    // Binds the shared IIntegrationEventPublisher abstraction to the RabbitMQ bus, so feature
    // handlers publish integration events without taking a dependency on MassTransit.
    public sealed class MassTransitEventPublisher : IIntegrationEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class
            => _publishEndpoint.Publish(@event, cancellationToken);
    }
}
