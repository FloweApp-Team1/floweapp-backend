using MassTransit;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Events.PaymentEvents;
using Shared.Interfaces;

namespace OrdersService.Features.Payments.PaymentWebhook.Consumers
{
    public class OrderPaymentSucceededConsumer : IConsumer<OrderPaymentSucceededEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderPaymentSucceededConsumer> _logger;
        private readonly global::Shared.Contracts.IEmailService _emailService;

        public OrderPaymentSucceededConsumer(IUnitOfWork unitOfWork, ILogger<OrderPaymentSucceededConsumer> logger, global::Shared.Contracts.IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<OrderPaymentSucceededEvent> context)
        {
            var message = context.Message;
            var orderRepo = _unitOfWork.Repository<Order>();
            var order = await orderRepo.GetByIdAsync(message.OrderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when processing payment success.", message.OrderId);
                return;
            }

            if (order.LastPaymentAttemptId != message.PaymentAttemptId)
            {
                _logger.LogInformation(
                    "Ignored stale or mismatched OrderPaymentSucceededEvent for Order {OrderId}. Expected Attempt: {ExpectedAttempt}, Got: {GotAttempt}",
                    order.Id, order.LastPaymentAttemptId, message.PaymentAttemptId);
                return;
            }

            if (order.PaymentStatus != PaymentStatusEnum.Paid)
            {
                order.PaymentStatus = PaymentStatusEnum.Paid;
                orderRepo.Update(order);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                
                _logger.LogInformation("Order {OrderId} marked as Paid.", order.Id);

                try
                {
                    if (!string.IsNullOrWhiteSpace(message.CustomerEmail))
                    {
                        string body = $"Hello,\n\nYour card payment for order {order.OrderNumber} was successful.\nTotal: {order.Total:C}\n\nThank you for shopping with Flowers App!";
                        await _emailService.SendAsync(message.CustomerEmail, "Payment Successful - Flowers App", body, context.CancellationToken);
                    }
                    else
                    {
                        // Note: A user repository does not exist in OrdersService. Since HTTP calls to IdentityService are forbidden to keep it decoupled, we skip the email if Stripe didn't provide it.
                        _logger.LogWarning("Cannot send order confirmation email for order {OrderId} because CustomerEmail is missing in the event.", order.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send order confirmation email for order {OrderId}", order.Id);
                }
            }
        }
    }
}
