using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using IdentityService.Features.Admin.DriverApplications.GetDriverApplication.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Admin.DriverApplications.GetDriverApplication;

public class GetDriverApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/drivers/applications/{driverId:guid}", async (Guid driverId, [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDriverApplicationDetailsQuery(driverId));

            if (result.IsSuccess)
            {
                var _deiverApplicationDetailsVM = new DriverApplicationDetailsVM
                {
                    Id = result.Value.Id.ToString(),
                    Name = result.Value.Name,
                    Email = result.Value.Email,
                    Gender = result.Value.Gender.ToString(),
                    Nid = result.Value.Nid,
                    LicenceImageUrl = result.Value.LicenceImageUrl,
                    NidImageUrl = result.Value.NidImageUrl,
                    Phone = result.Value.Phone,
                    SubmittedAt = result.Value.SubmittedAt,
                    Status = result.Value.Status.ToString(),
                    VehicleCapacity = result.Value.VehicleCapacity,
                    VehiclePlateNumber = result.Value.VehiclePlateNumber,
                    VehicleType = result.Value.VehicleType.ToString(),
                };
                return Results.Ok(ApiResponse<DriverApplicationDetailsVM>.Success(_deiverApplicationDetailsVM));

            }
            return Results.NotFound(ApiResponse<DriverApplicationDetailsVM>.Fail(result.Error));

        })
            .WithTags("Admin")
            .WithName("GetDriverApplication");
            //.RequireAuthorization("AdminOnly");
    }
}
