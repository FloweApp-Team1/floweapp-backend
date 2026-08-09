using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using IdentityService.Common.Security;

namespace IdentityService.Features.Admin.DriverApplications.ReviewDriverApplication;

public class ReviewDriverApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/drivers/applications/{driverId:guid}/status", (Guid driverId) =>
                ApiResponse.Success(new { driverId }, "Driver application reviewed").ToHttpResult())
            .WithTags("Admin")
            .WithName("ReviewDriverApplication")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
