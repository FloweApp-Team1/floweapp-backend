using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.HomeSections.ReorderHomeSections;

public class ReorderHomeSectionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/home-sections/reorder", () =>
                ApiResponse.Success(new { }, "Home sections reordered").ToHttpResult())
            .WithTags("Admin")
            .WithName("ReorderHomeSections")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
