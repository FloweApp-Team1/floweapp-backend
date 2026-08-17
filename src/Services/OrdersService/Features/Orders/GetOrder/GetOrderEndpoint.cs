using Shared.Contracts;
using Shared.Responses;

namespace OrdersService.Features.Orders.GetOrder;

public class GetOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}", (Guid orderId) =>
                ApiResponse.Success(new { }, "Order retrieved").ToHttpResult())
            .WithTags("Orders")
            .WithName("GetOrderDetails")
            // Any authenticated role, not just AppPolicies.CustomerOnly: the contract makes
            // this endpoint accessible to the order's own customer AND its assigned driver.
            .RequireAuthorization();
    }
}
