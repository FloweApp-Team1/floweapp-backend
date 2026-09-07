using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.Orders.AdminUpdateOrderStatus;

public class AdminUpdateOrderStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/orders/{orderId:guid}/status", async (
                Guid orderId,
                [FromBody] AdminUpdateOrderStatusRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new AdminUpdateOrderStatusCommand(
                    orderId,
                    request.Status,
                    request.Note),
                cancellationToken);

            return result.ToMinimalApiResult(
                "Order status updated");
        })
            .WithTags("Orders")
            .WithName("AdminUpdateOrderStatus")
            .RequireAuthorization(AppPolicies.AdminOnly)
            .Produces<ApiResponse<AdminUpdateOrderStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}