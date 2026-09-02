using IdentityService.Common.Interfaces;
using IdentityService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Events.DriverDelivery;
using Shared.Interfaces;
using System.Text.Json;

namespace IdentityService.Infrastructure.Messaging.Consumers
{
    public class DriverLocationUpdatedConsumer : IConsumer<DriverLocationUpdatedEvent>
    {
        private readonly ILogger<DriverLocationUpdatedConsumer> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFcmService _fcmService;

        public DriverLocationUpdatedConsumer(
            ILogger<DriverLocationUpdatedConsumer> logger,
            IUnitOfWork unitOfWork,
            IFcmService fcmService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _fcmService = fcmService;
        }

        public async Task Consume(ConsumeContext<DriverLocationUpdatedEvent> context)
        {
            var message = context.Message;

            var tokens = await _unitOfWork.Repository<UserDeviceToken>()
                .Query()
                .Where(t => t.UserId == message.CustomerId)
                .Select(t => t.FcmToken)
                .ToListAsync(context.CancellationToken);

            if (tokens.Count == 0)
            {
                _logger.LogDebug("No FCM tokens found for Customer {CustomerId}. Skipping driver location update.", message.CustomerId);
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "type", "driver_location_update" },
                { "orderId", message.OrderId.ToString() },
                { "lat", message.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                { "lng", message.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                { "timestamp", message.RecordedAt.ToString("O") }
            };

            await _fcmService.SendSilentDataMessageAsync(tokens, data, context.CancellationToken);
            
            _logger.LogInformation("Successfully sent FCM message for Driver Location Update (Order {OrderId}) to {TokenCount} device(s).", 
                message.OrderId, tokens.Count);
        }
    }
}
