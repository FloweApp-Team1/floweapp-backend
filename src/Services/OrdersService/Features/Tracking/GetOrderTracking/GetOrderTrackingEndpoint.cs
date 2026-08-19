using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.Tracking.GetOrderTracking;

public class GetOrderTrackingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}/tracking", (Guid orderId) =>
                ApiResponse.Success(new { }, "Order tracking retrieved").ToHttpResult())
            .WithTags("Orders")
            .WithName("GetOrderTracking")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
