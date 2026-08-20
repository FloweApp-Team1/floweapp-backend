using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrders;

public class GetAssignedOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/drivers/me/orders", () =>
                ApiResponse.Success(new { }, "Assigned orders retrieved").ToHttpResult())
            .WithTags("Driver Fulfillment")
            .WithName("GetDriverOrders")
            .RequireAuthorization(AppPolicies.DriverApproved);
    }
}
