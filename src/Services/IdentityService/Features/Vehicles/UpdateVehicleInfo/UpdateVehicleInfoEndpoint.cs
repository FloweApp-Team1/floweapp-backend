using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;

namespace IdentityService.Features.Vehicles.UpdateVehicleInfo;

public class UpdateVehicleInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/vehicles/info",
            async Task<IResult> (
                [FromForm] UpdateVehicleInfoVM request,
                [FromServices] IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateVehicleInfoCommand(
                    request.VehicleTypeId,
                    request.PlateNumber,
                    request.LicenseDocument);

                var result = await mediator.Send(command, cancellationToken);

                return result.ToMinimalApiResult("Vehicle info updated successfully.");
            })
            .WithTags("Vehicles")
            .WithName("UpdateVehicleInfo")
            .RequireAuthorization()
            .DisableAntiforgery() // multipart/form-data endpoint
            .Produces<ApiResponse<UpdateVehicleInfoResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
