using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
                IHostEnvironment hostEnvironment,
                ISender sender,
                ILogger<StripeWebhookEndpoint> logger,
                CancellationToken cancellationToken) =>
            {
                var verifyWebhookSignature = configuration.GetValue(
                    "Stripe:VerifyWebhookSignature",
                    defaultValue: true);

                // Read the raw body manually to ensure signature verification succeeds
                var rawBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync(cancellationToken);

                try
                {
                    Event stripeEvent;

                    if (verifyWebhookSignature)
                    {
                        var secret = configuration["Stripe:WebhookSecret"];
                        if (string.IsNullOrEmpty(secret))
                        {
                            logger.LogError("Stripe webhook secret is missing while signature verification is enabled.");
                            return Results.StatusCode(500);
                        }

                        var stripeSignature = httpContext.Request.Headers["Stripe-Signature"].FirstOrDefault();
                        if (string.IsNullOrEmpty(stripeSignature))
                        {
                            logger.LogWarning("Missing Stripe-Signature header.");
                            return Results.BadRequest("Missing Stripe-Signature header.");
                        }

                        stripeEvent = EventUtility.ConstructEvent(
                            json: rawBody,
                            stripeSignatureHeader: stripeSignature,
                            secret: secret,
                            throwOnApiVersionMismatch: false
                        );
                    }
                    else
                    {
                        if (!hostEnvironment.IsDevelopment())
                        {
                            logger.LogCritical(
                                "Stripe webhook signature verification cannot be disabled outside Development.");
                            return Results.StatusCode(500);
                        }

                        logger.LogWarning(
                            "Stripe webhook signature verification is disabled for local Development. " +
                            "Never use this setting in a deployed environment.");

                        stripeEvent = EventUtility.ParseEvent(
                            json: rawBody,
                            throwOnApiVersionMismatch: false);
                    }

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
