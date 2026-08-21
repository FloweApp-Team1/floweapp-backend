using Shared.Contracts;
using Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrdersService.Features.Payments.RetryPayment
{
    public class RetryPaymentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/{orderId:guid}/retry-payment", async Task<IResult> (Guid orderId, ISender sender) =>
            {
                var command = new RetryPaymentCommand(orderId);
                var result = await sender.Send(command);

                if (result.IsSuccess)
                {
                    return ApiResponse.Success(result.Value, "Retry initiated").ToHttpResult();
                }

                return ApiResponse.Fail(result.Error.Message, StatusCodes.Status400BadRequest).ToHttpResult();
            })
            .RequireAuthorization("CustomerOnly")
            .WithName("RetryPayment")
            .WithTags("Orders");
        }
    }
}
