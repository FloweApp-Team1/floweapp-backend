using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;

namespace IdentityService.Features.Vehicles.GetVehicles;

public class GetVehiclesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/vehicles", async ([AsParameters] PaginationRequest request,CancellationToken cancellationToken,[FromServices]IMediator mediator) =>
        {
            var result = await mediator.Send(new GetVehiclesQuery(request), cancellationToken);
            if (result.IsSuccess)
            {
                return Results.Ok(ApiResponse.Paginated(result.Value.Items, result.Value.TotalCount,request));
            }
            return Results.NotFound(ApiResponse.Fail(result.Error.Message));
        })
            .WithTags("Vehicles")
            .WithName("GetVehicles")
            .AllowAnonymous();
    }
}
