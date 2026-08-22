using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace CatalogService.Features.Admin.HomeSections.ReorderHomeSections;
public class ReorderHomeSectionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/home-sections/reorder", async (
                ReorderHomeSectionsBody body,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var command = new ReorderHomeSectionsCommand(body.Sections);
            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Home sections reordered");
        })
            .WithTags("Admin")
            .WithName("ReorderHomeSections")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}

public record ReorderHomeSectionsBody(List<SectionOrder> Sections);