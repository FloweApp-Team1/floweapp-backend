using MassTransit;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Events.PaymentEvents;
using Shared.Interfaces;
using Stripe;

namespace PaymentService.Features.Webhook
{
    public class StripeWebhookCommandHandler : IRequestHandler<StripeWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<StripeWebhookCommandHandler> _logger;

        public StripeWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            ILogger<StripeWebhookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Handle(StripeWebhookCommand request, CancellationToken cancellationToken)
        {
            var stripeEvent = request.StripeEvent;

            // 1. Save WebhookEvent idempotently
            var webhookEvent = new WebhookEvent
            {
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                ReceivedAt = DateTimeOffset.UtcNow,
                Payload = request.RawJson,
                Processed = false
            };

            try
            {
                await _unitOfWork.Repository<WebhookEvent>().AddAsync(webhookEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                _logger.LogInformation("Webhook event {EventId} was already processed.", stripeEvent.Id);
                return; // Idempotently ignore
            }

            // 2. Process event based on type
            if (stripeEvent.Type == "checkout.session.completed" || 
                stripeEvent.Type == "checkout.session.expired" ||
                stripeEvent.Type == "checkout.session.async_payment_failed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session == null)
                {
                    _logger.LogError("Stripe event {EventId} data object was not a Session.", stripeEvent.Id);
                    return;
                }

                var paymentAttempt = await _unitOfWork.Repository<PaymentAttempt>()
                    .Query()
                    .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id, cancellationToken);

                if (paymentAttempt == null)
                {
                    _logger.LogError("Received webhook for unknown Stripe Session ID {SessionId}", session.Id);
                    return; // Return 200 OK so Stripe doesn't retry a data mismatch
                }

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    paymentAttempt.Status = PaymentStatus.Paid;
                    paymentAttempt.CompletedAt = DateTimeOffset.UtcNow;
                    paymentAttempt.StripePaymentIntentId = session.PaymentIntentId;

                    await _publishEndpoint.Publish(new OrderPaymentSucceededEvent
                    {
                        OrderId = paymentAttempt.OrderId,
                        PaymentAttemptId = paymentAttempt.Id,
                        AmountTotal = paymentAttempt.AmountTotal,
                        Currency = paymentAttempt.Currency
                    }, cancellationToken);
                }
                else if (stripeEvent.Type == "checkout.session.expired" || 
                         stripeEvent.Type == "checkout.session.async_payment_failed")
                {
                    paymentAttempt.Status = stripeEvent.Type == "checkout.session.expired" 
                        ? PaymentStatus.Expired 
                        : PaymentStatus.Failed;

                    await _publishEndpoint.Publish(new OrderPaymentFailedEvent
                    {
                        OrderId = paymentAttempt.OrderId,
                        Reason = stripeEvent.Type
                    }, cancellationToken);
                }

                // 3. Mark processed and atomically save changes
                webhookEvent.Processed = true;
                _unitOfWork.Repository<PaymentAttempt>().Update(paymentAttempt);
                _unitOfWork.Repository<WebhookEvent>().Update(webhookEvent);
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // We just mark it as processed since we don't care about other event types currently
                webhookEvent.Processed = true;
                _unitOfWork.Repository<WebhookEvent>().Update(webhookEvent);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
