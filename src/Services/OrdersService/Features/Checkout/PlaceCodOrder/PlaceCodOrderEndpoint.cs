using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.Checkout.PlaceCodOrder;

public class PlaceCodOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/cod", () =>
                ApiResponse.Success(new { }, "Order placed").ToHttpResult())
            .WithTags("Orders")
            .WithName("PlaceCodOrder")
            .RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
