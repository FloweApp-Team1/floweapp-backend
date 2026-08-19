using Shared.Interfaces;

namespace OrdersService.Infrastructure.Messaging
{
    // Stand-in used when no broker is configured (RabbitMq__Host unset). Publishing is
    // recorded in the log rather than silently dropped, so a missing silent push on the
    // tracking screen is traceable to a missing broker instead of looking like a code bug.
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
