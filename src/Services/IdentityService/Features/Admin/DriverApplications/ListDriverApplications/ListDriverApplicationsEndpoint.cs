using IdentityService.Common.Contracts;
using IdentityService.Common.Requests;
using IdentityService.Common.Responses;
using IdentityService.Common.Security;

namespace IdentityService.Features.Admin.DriverApplications.ListDriverApplications;

public class ListDriverApplicationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/drivers/applications", ([AsParameters] PaginationRequest request) =>
                ApiResponse.Paginated<object>([], totalCount: 0, request).ToHttpResult())
            .WithTags("Admin")
            .WithName("ListDriverApplications")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
