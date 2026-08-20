using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace CatalogService.Features.Admin.HomeSections.CreateHomeSection;
public class CreateHomeSectionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/home-sections", async (
                CreateHomeSectionCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Home section created");
        })
            .WithTags("Admin")
            .WithName("CreateHomeSection")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}