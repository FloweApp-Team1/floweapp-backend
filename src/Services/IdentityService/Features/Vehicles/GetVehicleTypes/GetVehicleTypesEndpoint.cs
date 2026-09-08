using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;

namespace IdentityService.Features.Vehicles.GetVehicleTypes;

public class GetVehicleTypesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/vehicle-types", async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetVehicleTypesQuery(), cancellationToken);
                return result.ToMinimalApiResult("Vehicle types retrieved");
            })
            .WithTags("Vehicles")
            .WithName("GetVehicleTypes")
            .AllowAnonymous()
            .Produces<ApiResponse<IReadOnlyList<VehicleTypeResponse>>>(StatusCodes.Status200OK);
    }
}
