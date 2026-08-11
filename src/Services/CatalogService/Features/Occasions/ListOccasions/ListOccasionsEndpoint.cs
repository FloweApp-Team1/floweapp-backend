using Shared.Contracts;
using Shared.Responses;

namespace CatalogService.Features.Occasions.ListOccasions;

public class ListOccasionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/occasions", () =>
                ApiResponse.Success(new { }, "Occasions retrieved").ToHttpResult())
            .WithTags("Occasions")
            .WithName("ListOccasions")
            .AllowAnonymous();
    }
}
