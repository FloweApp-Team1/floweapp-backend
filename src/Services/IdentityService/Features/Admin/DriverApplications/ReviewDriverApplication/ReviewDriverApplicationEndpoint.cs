using IdentityService.Common.Contracts;

namespace IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication;

public class ReviewDriverApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/drivers/applications/{driverId:guid}/status", (Guid driverId) => { })
            .WithTags("Admin")
            .WithName("ReviewDriverApplication")
            .RequireAuthorization("AdminOnly");
    }
}
