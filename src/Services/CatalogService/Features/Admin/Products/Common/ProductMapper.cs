using CatalogService.Domain.Entities;

namespace CatalogService.Features.Admin.Products.Common
{
    public static class ProductMapper
    {
        public static ProductDto ToDto(Product product)
        {
            var availableStock = product.StoreStocks?.Sum(s => s.Quantity) ?? product.StockQuantity;
            var discountPercent = product.DiscountPercent is > 0 ? product.DiscountPercent : null;
            var discountedPrice = discountPercent is null
                ? product.Price
                : Math.Round(product.Price * (1 - discountPercent.Value / 100m), 2);

            return new ProductDto(
                Id: product.Id,
                Name: product.Name,
                Price: product.Price,
                DiscountedPrice: discountedPrice,
                DiscountPercent: discountPercent,
                InStock: availableStock > 0,
                Images: product.ProductImages?
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .ToList() ?? new List<string>(),
                Description: product.Description,
                Includes: product.Includes ?? new List<string>(),
                AvailabilityStatus: availableStock > 0 ? "IN_STOCK" : "OUT_OF_STOCK",
                AvailableStock: availableStock,
                CategoryIds: new List<Guid> { product.CategoryId },
                OccasionIds: product.Occasions?.Select(o => o.Id).ToList() ?? new List<Guid>(),
                CatalogStatus: product.IsDeleted ? "ARCHIVED" : "ACTIVE",
                CreatedAt: product.CreatedAt,
                UpdatedAt: product.UpdatedAt,
                LastChangedBy: product.LastChangedBy);
        }
    }
}
