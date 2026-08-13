using CatalogService.Features.Categories.ListCategories.Dtos;
using MediatR;
using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;

namespace CatalogService.Features.Categories.ListCategories;

public class ListCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (
                [AsParameters] PaginationRequest pagination,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new ListCategoriesQuery(pagination), cancellationToken);
                return result.ToHttpResult();
            })
            .WithTags("Categories")
            .WithName("ListCategories")
            .Produces<ApiResponse<IReadOnlyList<ListCategoriesResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }
}
