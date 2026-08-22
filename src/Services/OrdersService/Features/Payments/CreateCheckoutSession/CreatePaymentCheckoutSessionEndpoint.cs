using Shared.Contracts;
using Shared.Responses;
using Shared.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
namespace OrdersService.Features.Payments.CreateCheckoutSession;

public class CreatePaymentCheckoutSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/checkout", async Task<IResult> ([FromBody] CreatePaymentCheckoutSessionCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            if (result.IsSuccess)
            {
                return ApiResponse.Success(result.Value, "Checkout session created").ToHttpResult();
            }
            return ApiResponse.Fail(result.Error.Message, StatusCodes.Status400BadRequest).ToHttpResult();
        })
        .WithTags("Orders")
        .WithName("CreatePaymentCheckoutSession")
        .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
