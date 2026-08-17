using MediatR;
using Shared.Results;

namespace PaymentService.Features.Checkout.CreateCheckoutSession.Commands
{
    public record CreateCheckoutSessionCommand(Guid OrderId, long AmountTotal, string Currency) : IRequest<Result<CreateCheckoutSessionResponse>>;
}
