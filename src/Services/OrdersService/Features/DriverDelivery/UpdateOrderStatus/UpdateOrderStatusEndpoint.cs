using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus;

public class UpdateOrderStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/orders/{orderId:guid}/status", async (
                Guid orderId,
                [FromBody] UpdateOrderStatusRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new UpdateOrderStatusCommand(orderId, request.Status, request.Note),
                    cancellationToken);

                return result.ToMinimalApiResult("Order status updated");
            })
            // Tagged "Orders" (not "Driver Fulfillment") to match the contract's operation
            // tag, even though the feature lives under Features/DriverDelivery and requires
            // the Driver role.
            .WithTags("Orders")
            .WithName("UpdateOrderStatus")
            .RequireAuthorization(AppPolicies.DriverApproved)
            .Produces<ApiResponse<UpdateOrderStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}
