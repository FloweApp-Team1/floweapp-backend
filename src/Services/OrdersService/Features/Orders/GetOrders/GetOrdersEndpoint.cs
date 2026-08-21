using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.Orders.GetOrders;

public class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () =>
                ApiResponse.Success(new { }, "Orders retrieved").ToHttpResult())
            .WithTags("Orders")
            .WithName("GetOrders")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
