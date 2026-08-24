using MassTransit;
using Shared.Contracts;
using Shared.Events.OrderEvents;

namespace OrdersService.Features.Checkout.Consumers
{
    public class SendOrderConfirmationEmailConsumer : IConsumer<OrderConfirmedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<SendOrderConfirmationEmailConsumer> _logger;

        public SendOrderConfirmationEmailConsumer(IEmailService emailService, ILogger<SendOrderConfirmationEmailConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
        {
            var message = context.Message;
            
            if (string.IsNullOrWhiteSpace(message.UserEmail))
            {
                _logger.LogWarning("Cannot send order confirmation email for order {OrderId} because UserEmail is missing.", message.OrderId);
                return;
            }

            try
            {
                string subject;
                string paymentText;

                if (string.Equals(message.PaymentMethod, "Cod", StringComparison.OrdinalIgnoreCase))
                {
                    subject = "Order Confirmed - Flowers App";
                    paymentText = "Your Cash on Delivery order";
                }
                else
                {
                    subject = "Payment Successful - Flowers App";
                    paymentText = "Your card payment for order";
                }

                string body = $"Hello,\n\n{paymentText} {message.OrderNumber} was successful.\nTotal: {message.Total:C}\n\nThank you for shopping with Flowers App!";
                
                // Use MessageId or OrderId for deduplication header
                var messageId = context.MessageId?.ToString() ?? message.OrderId.ToString();
                
                await _emailService.SendAsync(message.UserEmail, subject, body, context.CancellationToken, messageId);

                _logger.LogInformation("Order confirmation email dispatched for order {OrderId} to {Email}", message.OrderId, message.UserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", message.OrderId);
                throw; // Rethrow to allow MassTransit to apply its retry policy for transient errors
            }
        }
    }
}
