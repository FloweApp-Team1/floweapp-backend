using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;


namespace CatalogService.Features.Admin.Occasions.ArchiveOccasion;
public class ArchiveOccasionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/occasions/{occasionId:guid}", async (
                Guid occasionId,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ArchiveOccasionCommand(occasionId), cancellationToken);
            return result.ToMinimalApiResult("Occasion archived");
        })
            .WithTags("Admin")
            .WithName("ArchiveOccasion")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
