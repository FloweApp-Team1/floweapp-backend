using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using Shared.Security;

namespace IdentityService.Features.Vehicles.GetVehicleInfo;

public class GetVehicleInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/vehicles/info",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetVehicleInfoQuery(), cancellationToken);

                return result.ToMinimalApiResult("Vehicle info retrieved successfully.");
            })
            .WithTags("Vehicles")
            .WithName("GetVehicleInfo")
            .RequireAuthorization(AppPolicies.DriverApproved)
            .Produces<ApiResponse<GetVehicleInfoResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
