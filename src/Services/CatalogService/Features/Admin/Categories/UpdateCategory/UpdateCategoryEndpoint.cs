using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Categories.UpdateCategory;

public class UpdateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/categories/{id:guid}", (Guid id) =>
                ApiResponse.Success(new { }, "Category updated").ToHttpResult())
            .WithTags("Admin")
            .WithName("UpdateCategory")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
