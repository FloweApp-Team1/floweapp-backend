using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.Payments.CreateCheckoutSession;

public class CreatePaymentCheckoutSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/checkout-session", () =>
                ApiResponse.Success(new { }, "Checkout session created").ToHttpResult())
            .WithTags("Payments")
            .WithName("CreatePaymentCheckoutSession")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
