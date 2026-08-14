using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Categories.Common;
using CatalogService.Features.Admin.Occasions.Common;

namespace CatalogService.Features.Admin.HomeSections.Common
{
    public static class HomeSectionMapper
    {
        public static HomeSectionDto ToDto(HomeSection section) => new(
            section.Id,
            section.Type.ToString().ToLowerInvariant(),
            section.Title,
            section.Order,
            section.Enabled,
            section.ViewAllDeeplink,
            section.BannerImageUrl,
            section.BannerDeeplink,
            section.ProductSelectionRule?.ToString(),
            section.SectionCategories?.Select(sc => CategoryMapper.ToDto(sc.Category)).ToList() ?? new(),
            section.SectionProducts?
                .OrderBy(sp => sp.DisplayOrder)
                .Select(sp => ToProductSummary(sp.Product))
                .ToList() ?? new(),
            section.SectionOccasions?.Select(so => OccasionMapper.ToDto(so.Occasion)).ToList() ?? new(),
            section.UpdatedAt,
            section.LastChangedBy);

        private static ProductSummaryDto ToProductSummary(Product product)
        {
            var discountPercent = product.DiscountPercent is > 0 ? product.DiscountPercent : null;
            var discountedPrice = discountPercent is null
                ? product.Price
                : Math.Round(product.Price * (1 - discountPercent.Value / 100m), 2);
            var stock = product.StoreStocks?.Sum(s => s.Quantity) ?? product.StockQuantity;

            return new ProductSummaryDto(
                product.Id,
                product.Name,
                product.Price,
                discountedPrice,
                discountPercent,
                stock > 0,
                product.ProductImages?.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault());
        }
    }
}
