using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;
using Shared.Results;
using Stripe;
using Stripe.Checkout;

using Shared.Interfaces;

namespace PaymentService.Features.Checkout.CreateCheckoutSession
{
    public class CreateCheckoutSessionHandler : IRequestHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public CreateCheckoutSessionHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<Result<CreateCheckoutSessionResponse>> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
        {
            var isAlreadyPaid = await _unitOfWork.Repository<PaymentAttempt>()
                .Query()
                .AnyAsync(p => p.OrderId == request.OrderId && p.Status == PaymentStatus.Paid, cancellationToken);
            
            if (isAlreadyPaid)
            {
                return Result<CreateCheckoutSessionResponse>.Failure(
                    new Error("Payment.AlreadyPaid", "This order has already been paid."));
            }

            var previousAttemptsCount = await _unitOfWork.Repository<PaymentAttempt>()
                .Query()
                .CountAsync(p => p.OrderId == request.OrderId, cancellationToken);
                
            int attemptNumber = previousAttemptsCount + 1;

            var scheme = _configuration["Frontend:DeepLinkScheme"] ?? "myapp";
            var successUrl = $"{scheme}://payment-success?orderId={request.OrderId}";
            var cancelUrl = $"{scheme}://payment-failed?orderId={request.OrderId}";

            var options = new SessionCreateOptions
            {
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Mode = "payment",
                PaymentMethodTypes = new List<string> { "card" },
                ClientReferenceId = request.OrderId.ToString(),
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = request.AmountTotal,
                            Currency = request.Currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order #{request.OrderId}",
                            },
                        },
                        Quantity = 1,
                    },
                },
            };

            var requestOptions = new RequestOptions 
            { 
                IdempotencyKey = $"checkout_{request.OrderId}_{attemptNumber}" 
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options, requestOptions, cancellationToken);

            var paymentAttempt = new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                StripeSessionId = session.Id,
                StripePaymentIntentId = null, // Set later via webhook when intent is created/confirmed
                SessionUrl = session.Url,
                AmountTotal = request.AmountTotal,
                Currency = request.Currency,
                Status = PaymentStatus.Pending,
                AttemptNumber = attemptNumber,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = session.ExpiresAt,
                CompletedAt = null
            };

            await _unitOfWork.Repository<PaymentAttempt>().AddAsync(paymentAttempt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateCheckoutSessionResponse(
                CheckoutUrl: session.Url,
                StripeSessionId: session.Id,
                PaymentAttemptId: paymentAttempt.Id
            );

            return Result<CreateCheckoutSessionResponse>.Success(response);
        }
    }
}
