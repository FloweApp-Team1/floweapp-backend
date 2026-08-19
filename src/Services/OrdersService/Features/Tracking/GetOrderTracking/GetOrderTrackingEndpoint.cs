using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace OrdersService.Features.Tracking.GetOrderTracking;

public class GetOrderTrackingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}/tracking", async (
                Guid orderId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetOrderTrackingQuery(orderId), cancellationToken);

                return result.ToMinimalApiResult("Order tracking retrieved");
            })
            .WithTags("Orders")
            .WithName("GetOrderTracking")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
