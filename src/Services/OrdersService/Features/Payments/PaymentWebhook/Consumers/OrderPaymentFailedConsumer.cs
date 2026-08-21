using MassTransit;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Events.PaymentEvents;
using Shared.Interfaces;

namespace OrdersService.Features.Payments.PaymentWebhook.Consumers
{
    public class OrderPaymentFailedConsumer : IConsumer<OrderPaymentFailedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderPaymentFailedConsumer> _logger;

        public OrderPaymentFailedConsumer(IUnitOfWork unitOfWork, ILogger<OrderPaymentFailedConsumer> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPaymentFailedEvent> context)
        {
            var message = context.Message;
            var orderRepo = _unitOfWork.Repository<Order>();
            var order = await orderRepo.GetByIdAsync(message.OrderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when processing payment failure.", message.OrderId);
                return;
            }

            // Concurrency Check: Only process if the webhook attempt matches the LastPaymentAttemptId
            if (order.LastPaymentAttemptId != message.PaymentAttemptId)
            {
                _logger.LogInformation(
                    "Ignored stale or mismatched OrderPaymentFailedEvent for Order {OrderId}. Expected Attempt: {ExpectedAttempt}, Got: {GotAttempt}",
                    order.Id, order.LastPaymentAttemptId, message.PaymentAttemptId);
                return;
            }

            if (order.PaymentStatus != PaymentStatusEnum.Failed)
            {
                order.PaymentStatus = PaymentStatusEnum.Failed;
                orderRepo.Update(order);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                
                _logger.LogInformation("Order {OrderId} marked as Failed.", order.Id);
            }
        }
    }
}
