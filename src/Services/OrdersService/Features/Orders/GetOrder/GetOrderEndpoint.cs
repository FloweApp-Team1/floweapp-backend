using Shared.Contracts;
using Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrdersService.Features.Orders.GetOrder;

public class GetOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/{orderId:guid}", async Task<IResult> (Guid orderId, ISender sender) =>
        {
            var result = await sender.Send(new GetOrderQuery(orderId));
            if (result.IsSuccess)
            {
                return ApiResponse.Success(result.Value, "Order retrieved").ToHttpResult();
            }
            return ApiResponse.Fail(result.Error.Message, StatusCodes.Status400BadRequest).ToHttpResult();
        })
        .WithTags("Orders")
        .WithName("GetOrderDetails")
        .RequireAuthorization();
    }
}
