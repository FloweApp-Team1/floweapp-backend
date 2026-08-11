using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Categories.ArchiveCategory;

public class ArchiveCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/categories/{id:guid}", (Guid id) =>
                ApiResponse.Success(new { }, "Category archived").ToHttpResult())
            .WithTags("Admin")
            .WithName("ArchiveCategory")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
