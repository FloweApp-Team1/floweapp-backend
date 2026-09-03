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
                [FromQuery] OrderStatusEnum? status,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAssignedOrdersQuery(request, status), cancellationToken);

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
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}
