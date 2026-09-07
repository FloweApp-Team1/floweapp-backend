using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrderDetails;

public class GetAssignedOrderDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/drivers/me/orders/{orderId:guid}", async Task<IResult> (
                Guid orderId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAssignedOrderDetailsQuery(orderId), cancellationToken);

                return result.ToMinimalApiResult("Assigned order retrieved");
            })
            .WithTags("Driver Fulfillment")
            .WithName("GetDriverOrderDetails")
            .RequireAuthorization(AppPolicies.DriverApproved)
            .Produces<ApiResponse<GetAssignedOrderDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
