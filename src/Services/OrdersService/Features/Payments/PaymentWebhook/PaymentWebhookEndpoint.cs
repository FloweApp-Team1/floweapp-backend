using Shared.Contracts;
using Shared.Responses;

namespace OrdersService.Features.Payments.PaymentWebhook;

public class PaymentWebhookEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/webhook", () =>
                ApiResponse.Success(new { }, "Webhook processed").ToHttpResult())
            .WithTags("Payments")
            .WithName("HandlePaymentWebhook")
            // Server-to-server callback, authenticated via the X-Gateway-Signature HMAC
            // instead of a customer JWT - not called by client apps.
            .AllowAnonymous();
    }
}
