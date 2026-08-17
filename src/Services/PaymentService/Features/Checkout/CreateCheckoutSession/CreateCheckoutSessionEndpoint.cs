using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace PaymentService.Features.Checkout.CreateCheckoutSession
{
    public class CreateCheckoutSessionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/checkout", async (
                [FromBody] CreateCheckoutSessionCommand command, 
                ISender sender, 
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);
                
                return result.IsSuccess 
                    ? Results.Ok(result) 
                    : Results.BadRequest(result);
            })
            .WithTags("Checkout")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
