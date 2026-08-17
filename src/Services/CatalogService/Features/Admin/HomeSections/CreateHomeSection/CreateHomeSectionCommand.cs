using CatalogService.Features.Admin.HomeSections.Common;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Admin.HomeSections.CreateHomeSection
{
    public record CreateHomeSectionCommand(
     string Type,
     string Title,
     int Order,
     bool Enabled,
     string? ViewAllDeeplink,
     string? BannerImageUrl,
     string? BannerDeeplink,
     List<Guid>? CategoryIds,
     List<Guid>? OccasionIds,
     string? ProductSelectionRule,
     List<Guid>? ProductIds) : IRequest<Result<HomeSectionDto>>;
}
