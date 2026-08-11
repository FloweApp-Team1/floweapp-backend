using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Occasions.ArchiveOccasion;

public class ArchiveOccasionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/occasions/{id:guid}", (Guid id) =>
                ApiResponse.Success(new { }, "Occasion archived").ToHttpResult())
            .WithTags("Admin")
            .WithName("ArchiveOccasion")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
