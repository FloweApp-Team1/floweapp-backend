using IdentityService.Common.Interfaces;
using IdentityService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Events.OrderEvents;
using Shared.Interfaces;
using System.Text.Json;

namespace IdentityService.Infrastructure.Messaging.Consumers
{
    public class OrderStatusUpdatedConsumer : IConsumer<OrderStatusUpdatedEvent>
    {
        private readonly ILogger<OrderStatusUpdatedConsumer> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFcmService _fcmService;

        public OrderStatusUpdatedConsumer(
            ILogger<OrderStatusUpdatedConsumer> logger,
            IUnitOfWork unitOfWork,
            IFcmService fcmService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _fcmService = fcmService;
        }

        public async Task Consume(ConsumeContext<OrderStatusUpdatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation("Processing OrderStatusUpdatedEvent for Order {OrderId}. Status: {Status}", 
                message.OrderId, message.NewStatus);

            var tokens = await _unitOfWork.Repository<UserDeviceToken>()
                .Query()
                .Where(t => t.UserId == message.CustomerId)
                .Select(t => t.FcmToken)
                .ToListAsync(context.CancellationToken);

            if (tokens.Count == 0)
            {
                _logger.LogInformation("No FCM tokens found for Customer {CustomerId}. Skipping push notification.", message.CustomerId);
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "type", "order_status_update" },
                { "orderId", message.OrderId.ToString() },
                { "oldStatus", message.OldStatus },
                { "newStatus", message.NewStatus },
                { "timestamp", message.Timestamp.ToString("O") }
            };

            await _fcmService.SendSilentDataMessageAsync(tokens, data, context.CancellationToken);
            
            _logger.LogInformation("Successfully sent FCM message for Order {OrderId} to {TokenCount} device(s).", 
                message.OrderId, tokens.Count);
        }
    }
}
