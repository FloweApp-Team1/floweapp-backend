using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;
namespace CatalogService.Features.Admin.Categories.ArchiveCategory;
public class ArchiveCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/categories/{categoryId:guid}", async (
                Guid categoryId,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ArchiveCategoryCommand(categoryId), cancellationToken);
            return result.ToMinimalApiResult("Category archived");
        })
            .WithTags("Admin")
            .WithName("ArchiveCategory")
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}