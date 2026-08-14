using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace CatalogService.Features.Admin.HomeSections.ToggleHomeSectionStatus
{
    public class ToggleHomeSectionStatusEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("/admin/home-sections/{sectionId:guid}/status", async (
                    Guid sectionId,
                    ToggleHomeSectionStatusBody body,
                    ISender sender,
                    CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new ToggleHomeSectionStatusCommand(sectionId, body.Enabled), cancellationToken);
                return result.ToMinimalApiResult("Home section status updated");
            })
                .WithTags("Admin")
                .WithName("ToggleHomeSectionStatus")
                .RequireAuthorization(AppPolicies.AdminOnly);
        }
    }

    public record ToggleHomeSectionStatusBody(bool Enabled);
}
