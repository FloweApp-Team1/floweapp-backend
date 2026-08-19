using MediatR;
using Stripe;

namespace PaymentService.Features.Webhook
{
    public record StripeWebhookCommand(Event StripeEvent, string RawJson) : IRequest;

}
