using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;
using MediatR;

namespace CatalogService.Features.Occasions.ListOccasions;

public class ListOccasionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/occasions", async (
            [AsParameters] PaginationRequest request,
            CancellationToken cancellationToken,
            IMediator mediator) =>
        {
            var response = await mediator.Send(new ListOccasionsQuery(request), cancellationToken);

            if (!response.Status)
                return Results.Json(response, statusCode: response.Code);

            var paged = response.Data!;
            return Results.Ok(ApiResponse.Paginated(paged.Items, paged.TotalCount, request));
        })
            .WithTags("Occasions")
            .WithName("ListOccasions")
            .AllowAnonymous();
    }
}
