using MediatR;
using OrdersService.Features.DriverDelivery.GetAssignedOrderDetails;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.GetCurrentAssignedOrder;

public sealed class GetCurrentAssignedOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/drivers/me/assigned-order", async Task<IResult> (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCurrentAssignedOrderQuery(), cancellationToken);
            return result.ToMinimalApiResult("Current assigned order retrieved");
        })
        .WithTags("Driver Fulfillment")
        .WithName("GetCurrentDriverAssignedOrder")
        .RequireAuthorization(AppPolicies.DriverApproved)
        .Produces<ApiResponse<GetAssignedOrderDetailsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
