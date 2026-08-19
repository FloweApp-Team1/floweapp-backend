using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace OrdersService.Features.DriverDelivery.UpdateDriverLocation;

public class UpdateDriverLocationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/drivers/me/location", async (
                UpdateDriverLocationRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateDriverLocationCommand(request.Lat, request.Lng, request.RecordedAt);
                var result = await sender.Send(command, cancellationToken);

                return result.ToMinimalApiResult("Location reported");
            })
            .WithTags("Driver Fulfillment")
            .WithName("ReportDriverLocation")
            .RequireAuthorization(AppPolicies.DriverApproved);
    }
}
