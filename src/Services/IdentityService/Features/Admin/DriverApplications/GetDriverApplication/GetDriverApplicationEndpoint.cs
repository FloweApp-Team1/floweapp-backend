using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Admin.DriverApplications.GetDriverApplication;

public class GetDriverApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/drivers/applications/{driverId:guid}", (Guid driverId) =>
                ApiResponse.Success(new { driverId }, "Driver application retrieved").ToHttpResult())
            .WithTags("Admin")
            .WithName("GetDriverApplication")
            .RequireAuthorization("AdminOnly");
    }
}
