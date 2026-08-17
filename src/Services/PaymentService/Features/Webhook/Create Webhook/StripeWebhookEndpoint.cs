using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Stripe;

namespace PaymentService.Features.Webhook
{
    public class StripeWebhookEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/webhook", async (
                HttpContext httpContext,
                IConfiguration configuration,
                ISender sender,
                ILogger<StripeWebhookEndpoint> logger,
                CancellationToken cancellationToken) =>
            {
                var secret = configuration["Stripe:WebhookSecret"];
                if (string.IsNullOrEmpty(secret))
                {
                    logger.LogError("Stripe webhook secret is missing.");
                    return Results.StatusCode(500);
                }

                // Read the raw body manually to ensure signature verification succeeds
                var rawBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync(cancellationToken);

                try
                {
                    var stripeSignature = httpContext.Request.Headers["Stripe-Signature"].FirstOrDefault();
                    if (string.IsNullOrEmpty(stripeSignature))
                    {
                        logger.LogWarning("Missing Stripe-Signature header.");
                        return Results.BadRequest("Missing Stripe-Signature header.");
                    }

                    var stripeEvent = EventUtility.ConstructEvent(
                        json: rawBody,
                        stripeSignatureHeader: stripeSignature,
                        secret: secret,
                        throwOnApiVersionMismatch: false
                    );

                    var command = new StripeWebhookCommand(stripeEvent, rawBody);
                    await sender.Send(command, cancellationToken);

                    return Results.Ok();
                }
                catch (StripeException e)
                {
                    logger.LogError(e, "Stripe signature verification failed or invalid payload.");
                    return Results.BadRequest();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An unexpected error occurred while processing Stripe webhook.");
                    return Results.StatusCode(500);
                }
            });
        }
    }
}
