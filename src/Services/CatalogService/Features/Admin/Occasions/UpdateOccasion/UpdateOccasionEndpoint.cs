using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Occasions.UpdateOccasion;

public class UpdateOccasionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/occasions/{id:guid}", (Guid id) =>
                ApiResponse.Success(new { }, "Occasion updated").ToHttpResult())
            .WithTags("Admin")
            .WithName("UpdateOccasion")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
