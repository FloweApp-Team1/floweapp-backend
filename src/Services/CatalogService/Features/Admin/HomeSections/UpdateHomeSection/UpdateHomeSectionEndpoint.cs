using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;


namespace CatalogService.Features.Admin.HomeSections.UpdateHomeSection
{
    public class UpdateHomeSectionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/admin/home-sections/{sectionId:guid}", async (
                    Guid sectionId,
                    UpdateHomeSectionBody body,
                    ISender sender,
                    CancellationToken cancellationToken) =>
            {
                var command = new UpdateHomeSectionCommand(
                    sectionId,
                    body.Title,
                    body.Order,
                    body.Enabled,
                    body.ViewAllDeeplink,
                    body.BannerImageUrl,
                    body.BannerDeeplink,
                    body.CategoryIds,
                    body.OccasionIds,
                    body.ProductSelectionRule,
                    body.ProductIds);

                var result = await sender.Send(command, cancellationToken);
                return result.ToMinimalApiResult("Home section updated");
            })
                .WithTags("Admin")
                .WithName("UpdateHomeSection")
                .RequireAuthorization(AppPolicies.AdminOnly);
        }
    }
    public record UpdateHomeSectionBody(
        string? Title,
        int? Order,
        bool? Enabled,
        string? ViewAllDeeplink,
        string? BannerImageUrl,
        string? BannerDeeplink,
        List<Guid>? CategoryIds,
        List<Guid>? OccasionIds,
        string? ProductSelectionRule,
        List<Guid>? ProductIds);
}
