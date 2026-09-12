using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Domain.Enums;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Requests;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrders;

public class GetAssignedOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/drivers/me/orders", async Task<IResult> (
                [AsParameters] PaginationRequest request,
                [FromQuery] string? status,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                OrderStatusEnum? parsedStatus = null;

                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (!OrderStatusExtensions.TryParseContract(status, out var value))
                    {
                        return ApiResponse.Fail(
                            $"Unknown order status '{status}'.",
                            StatusCodes.Status400BadRequest,
                            [new ApiError(
                                "Use PLACED, PREPARING, PICKED_UP, OUT_FOR_DELIVERY, " +
                                "AWAITING_DELIVERY_CONFIRMATION, DELIVERED, or CANCELLED.",
                                "status")]).ToHttpResult();
                    }

                    parsedStatus = value;
                }

                var result = await sender.Send(
                    new GetAssignedOrdersQuery(request, parsedStatus),
                    cancellationToken);

                return result.IsSuccess
                    ? ApiResponse.Paginated(
                        result.Value.Orders,
                        result.Value.TotalCount,
                        request,
                        "Assigned orders retrieved").ToHttpResult()
                    : result.ToMinimalApiResult();
            })
            .WithTags("Driver Fulfillment")
            .WithName("GetDriverOrders")
            .RequireAuthorization(AppPolicies.DriverApproved)
            .Produces<ApiResponse<IReadOnlyList<AssignedOrderDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}
