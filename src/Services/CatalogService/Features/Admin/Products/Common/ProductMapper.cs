using CatalogService.Domain.Entities;

namespace CatalogService.Features.Admin.Products.Common
{
    public static class ProductMapper
    {
        public static ProductDto ToDto(Product product)
        {
            var availableStock = product.StoreStocks?.Sum(s => s.Quantity) ?? product.StockQuantity;

            var discountedPrice = product.Discounts?
               .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow && !d.IsDeleted)
               .OrderByDescending(d => d.StartDate)
               .Select(d => (decimal?)(product.Price - (product.Price * d.Percentage / 100)))
               .FirstOrDefault();

            var discountPercent = product.Discounts?
                .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow && !d.IsDeleted)
                .OrderByDescending(d => d.StartDate)
                .Select(d => (decimal?)d.Percentage)
                .FirstOrDefault();
            return new ProductDto(
                Id: product.Id,
                Name: product.Name,
                Price: product.Price,
                DiscountedPrice: discountedPrice ?? product.Price,
                DiscountPercent: (int?)discountPercent,
                InStock: availableStock > 0,
                Images: product.ProductImages?
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .ToList() ?? new List<string>(),
                Description: product.Description,
                Includes: product.Includes?.Select(i => i.Name).ToList() ?? new List<string>(),
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
