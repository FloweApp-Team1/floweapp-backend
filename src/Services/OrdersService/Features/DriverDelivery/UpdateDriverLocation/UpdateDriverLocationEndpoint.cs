using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.UpdateDriverLocation;

public class UpdateDriverLocationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/drivers/me/location", () =>
                ApiResponse.Success(new { }, "Location reported").ToHttpResult())
            .WithTags("Driver Fulfillment")
            .WithName("ReportDriverLocation")
            .RequireAuthorization(AppPolicies.DriverApproved);
    }
}
