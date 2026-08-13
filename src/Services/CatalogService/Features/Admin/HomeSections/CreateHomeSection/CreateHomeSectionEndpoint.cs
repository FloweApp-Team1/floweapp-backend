using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.HomeSections.CreateHomeSection;

public class CreateHomeSectionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/home-sections", () =>
                ApiResponse.Success(new { }, "Home section created").ToHttpResult())
            .WithTags("Admin")
            .WithName("CreateHomeSection")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
