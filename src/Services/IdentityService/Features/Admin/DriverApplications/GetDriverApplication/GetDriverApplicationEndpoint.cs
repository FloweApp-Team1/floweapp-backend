using IdentityService.Common.Contracts;

namespace IdentityService.Features.Admin.DriverApplications.GetDriverApplication;

public class GetDriverApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/drivers/applications/{driverId:guid}", (Guid driverId) => { })
            .WithTags("Admin")
            .WithName("GetDriverApplication")
            .RequireAuthorization("AdminOnly");
    }
}
