using IdentityService.Common.Interfaces;
using IdentityService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.OrderEvents;
using Shared.Interfaces;

namespace IdentityService.Infrastructure.Messaging.Consumers;

public sealed class DeliveryConfirmedByCustomerConsumer(
    ILogger<DeliveryConfirmedByCustomerConsumer> logger,
    IUnitOfWork unitOfWork,
    IFcmService fcmService)
    : IConsumer<DeliveryConfirmedByCustomerEvent>
{
    public async Task Consume(ConsumeContext<DeliveryConfirmedByCustomerEvent> context)
    {
        var message = context.Message;

        var tokens = await unitOfWork.Repository<UserDeviceToken>()
            .Query()
            .Where(t => t.UserId == message.DriverId && t.NotificationsEnabled)
            .Select(t => t.FcmToken)
            .ToListAsync(context.CancellationToken);

        if (tokens.Count == 0)
        {
            logger.LogInformation(
                "No enabled FCM tokens found for Driver {DriverId}. Skipping delivery confirmation notification for Order {OrderId}.",
                message.DriverId,
                message.OrderId);
            return;
        }

        var data = new Dictionary<string, string>
        {
            ["type"] = "delivery_confirmed",
            ["orderId"] = message.OrderId.ToString(),
            ["orderNumber"] = message.OrderNumber,
            ["customerId"] = message.CustomerId.ToString(),
            ["newStatus"] = "Delivered",
            ["timestamp"] = message.ConfirmedAt.ToString("O")
        };

        await fcmService.SendNotificationAsync(
            tokens,
            "Delivery confirmed",
            $"The customer confirmed delivery of order {message.OrderNumber}.",
            data,
            context.CancellationToken);

        logger.LogInformation(
            "Sent delivery confirmation notification for Order {OrderId} to {TokenCount} driver device(s).",
            message.OrderId,
            tokens.Count);
    }
}
