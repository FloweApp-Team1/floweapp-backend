using Shared.Interfaces;

namespace AddressCartService.Infrastructure.Messaging
{
    public sealed class LoggingEventPublisher : IIntegrationEventPublisher
    {
        private readonly ILogger<LoggingEventPublisher> _logger;

        public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            _logger.LogInformation(
                "No message broker is configured; {EventType} was not published. Payload: {@Event}",
                typeof(TEvent).Name, @event);

            return Task.CompletedTask;
        }
    }
}
