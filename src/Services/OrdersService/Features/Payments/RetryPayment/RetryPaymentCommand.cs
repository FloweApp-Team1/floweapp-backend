using MediatR;
using Shared.Results;

namespace OrdersService.Features.Payments.RetryPayment
{
    public record RetryPaymentCommand(Guid OrderId) : IRequest<Result<RetryPaymentResponse>>;
    
    public record RetryPaymentResponse(string CheckoutUrl, string StripeSessionId, Guid PaymentAttemptId);
}
