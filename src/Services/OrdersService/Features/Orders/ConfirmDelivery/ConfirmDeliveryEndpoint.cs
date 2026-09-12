using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.Orders.ConfirmDelivery;

public sealed class ConfirmDeliveryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/{orderId:guid}/confirm-delivery", async Task<IResult> (
            Guid orderId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ConfirmDeliveryCommand(orderId), cancellationToken);
            return result.ToMinimalApiResult("Delivery confirmed");
        })
        .WithTags("Orders")
        .WithName("ConfirmOrderDelivery")
        .RequireAuthorization(AppPolicies.CustomerOnly)
        .Produces<ApiResponse<ConfirmDeliveryResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}
