using CatalogService.Features.Admin.Categories.Common;
using CatalogService.Features.Admin.Occasions.Common;

namespace CatalogService.Features.Admin.HomeSections.Common
{
    public record HomeSectionDto(
        Guid Id,
        string Type,
        string Title,
        int Order,
        bool Enabled,
        string? ViewAllDeeplink,
        string? BannerImageUrl,
        string? BannerDeeplink,
        string? ProductSelectionRule,
        List<CategoryDto> Categories,
        List<ProductSummaryDto> Products,
        List<OccasionDto> Occasions,
        DateTime UpdatedAt,
        Guid LastChangedBy);
}
