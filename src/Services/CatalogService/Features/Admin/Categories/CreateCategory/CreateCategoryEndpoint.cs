using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace CatalogService.Features.Admin.Categories.CreateCategory;

public class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/categories", () =>
                ApiResponse.Success(new { }, "Category created").ToHttpResult())
            .WithTags("Admin")
            .WithName("CreateCategory")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
