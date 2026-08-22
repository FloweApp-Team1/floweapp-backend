using MassTransit;
using Shared.Interfaces;

namespace AddressCartService.Infrastructure.Messaging
{
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
