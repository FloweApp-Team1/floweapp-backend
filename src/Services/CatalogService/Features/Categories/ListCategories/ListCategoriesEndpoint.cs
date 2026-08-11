using Shared.Contracts;
using Shared.Responses;

namespace CatalogService.Features.Categories.ListCategories;

public class ListCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", () =>
                ApiResponse.Success(new { }, "Categories retrieved").ToHttpResult())
            .WithTags("Categories")
            .WithName("ListCategories")
            .AllowAnonymous();
    }
}
