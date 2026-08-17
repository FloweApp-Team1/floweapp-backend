using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace CatalogService.Features.Admin.Occasions.CreateOccasion;
public class CreateOccasionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/occasions", async (
                [FromForm] CreateOccasionRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var command = new CreateOccasionCommand(request.Name, request.Order, request.Image);
            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Occasion created");
        })
            .WithTags("Admin")
            .WithName("CreateOccasion")
            .DisableAntiforgery()
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
