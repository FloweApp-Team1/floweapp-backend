using IdentityService.Common.Contracts;

namespace IdentityService.Features.Admin.DriverApplications.ListDriverApplications;

public class ListDriverApplicationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/drivers/applications", () => { })
            .WithTags("Admin")
            .WithName("ListDriverApplications")
            .RequireAuthorization("AdminOnly");
    }
}
