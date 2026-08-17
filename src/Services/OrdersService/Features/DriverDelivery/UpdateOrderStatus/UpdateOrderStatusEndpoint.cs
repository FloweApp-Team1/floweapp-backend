using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus;

public class UpdateOrderStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/orders/{orderId:guid}/status", (Guid orderId) =>
                ApiResponse.Success(new { }, "Order status updated").ToHttpResult())
            // Tagged "Orders" (not "Driver Fulfillment") to match the contract's operation tag,
            // even though the feature lives under Features/DriverDelivery and requires the
            // Driver role.
            .WithTags("Orders")
            .WithName("UpdateOrderStatus")
            .RequireAuthorization(AppPolicies.DriverApproved);
    }
}
