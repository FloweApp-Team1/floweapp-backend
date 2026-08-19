using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.ClaimOrder;

public class ClaimOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{orderId:guid}/claim", async (
                Guid orderId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new ClaimOrderCommand(orderId), cancellationToken);

                return result.ToMinimalApiResult("Order claimed");
            })
            .WithTags("Driver Fulfillment")
            .WithName("ClaimOrder")
            // Approved drivers only: claiming an order exposes the customer's delivery
            // address to whoever holds the token.
            .RequireAuthorization(AppPolicies.DriverApproved)
            .Produces<ApiResponse<ClaimOrderResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}
