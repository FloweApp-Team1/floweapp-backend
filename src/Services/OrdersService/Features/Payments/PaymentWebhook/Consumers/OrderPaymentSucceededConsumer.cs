using MassTransit;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Events.OrderEvents;
using Shared.Events.PaymentEvents;
using Shared.Interfaces;

namespace OrdersService.Features.Payments.PaymentWebhook.Consumers
{
    public class OrderPaymentSucceededConsumer : IConsumer<OrderPaymentSucceededEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderPaymentSucceededConsumer> _logger;

        public OrderPaymentSucceededConsumer(IUnitOfWork unitOfWork, ILogger<OrderPaymentSucceededConsumer> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                
                await context.Publish(new OrderConfirmedEvent
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    PaymentMethod = order.PaymentMethod.ToString(),
                    OrderNumber = order.OrderNumber,
                    Total = order.Total,
                    UserEmail = message.CustomerEmail
                }, context.CancellationToken);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                
                _logger.LogInformation("Order {OrderId} marked as Paid and OrderConfirmedEvent published.", order.Id);
            }
        }
    }
}
