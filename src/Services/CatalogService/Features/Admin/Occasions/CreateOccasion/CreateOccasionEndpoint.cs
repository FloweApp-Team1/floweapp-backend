using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Occasions.CreateOccasion;

public class CreateOccasionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/occasions", () =>
                ApiResponse.Success(new { }, "Occasion created").ToHttpResult())
            .WithTags("Admin")
            .WithName("CreateOccasion")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
