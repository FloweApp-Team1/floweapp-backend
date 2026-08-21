using Shared.Contracts;
using Shared.Responses;
using Shared.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrdersService.Features.Checkout.PlaceCodOrder;

public class PlaceCodOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/cod", async Task<IResult> ([FromBody] PlaceCodOrderCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            if (result.IsSuccess)
            {
                return ApiResponse.Success(result.Value, "Order placed").ToHttpResult();
            }
            return ApiResponse.Fail(result.Error.Message, StatusCodes.Status400BadRequest).ToHttpResult();
        })
        .WithTags("Orders")
        .WithName("PlaceCodOrder")
        .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
