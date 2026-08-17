using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace CatalogService.Features.Admin.Occasions.UpdateOccasion;
public class UpdateOccasionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/occasions/{occasionId:guid}", async (
                Guid occasionId,
                [FromForm] UpdateOccasionRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var command = new UpdateOccasionCommand(occasionId, request.Name, request.Order, request.Image);
            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Occasion updated");
        })
            .WithTags("Admin")
            .WithName("UpdateOccasion")
            .DisableAntiforgery()
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
