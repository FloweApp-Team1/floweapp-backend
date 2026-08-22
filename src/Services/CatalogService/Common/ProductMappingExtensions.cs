using CatalogService.Domain.Entities;
using CatalogService.Features.Products;
using System.Linq.Expressions;

namespace CatalogService.Common
{
    public static class ProductMappingExtensions
    {
        // storeId == null -> legacy aggregate stock (Product.StockQuantity), preserving
        // today's behavior for guests/no resolved store. storeId given -> availability comes
        // from that store's ProductStoreStock row (0/missing counts as unavailable, not
        // filtered out - the product still appears in the list, just marked out of stock).
        public static Expression<Func<Product, ProductListItemDto>> ToListItemDto(Guid? storeId) => p => new ProductListItemDto(
        p.Id,
        p.Name,
        p.Price,
        p.Discounts
                .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                .OrderByDescending(d => d.StartDate)
                .Select(d => (decimal?)d.Percentage)
                .FirstOrDefault(),
            p.Discounts
                .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                .OrderByDescending(d => d.StartDate)
                .Select(d => (decimal?)(p.Price - (p.Price * d.Percentage / 100)))
                .FirstOrDefault(),

        storeId == null
            ? p.StockQuantity > 0
            : p.StoreStocks!.Any(s => s.StoreId == storeId && s.Quantity > 0),
        p.CategoryId,
        p.Category != null ? p.Category.Name : null,

        p.ProductImages != null && p.ProductImages.Any()
            ? (p.ProductImages.FirstOrDefault(i => i.IsPrimary) ?? p.ProductImages.OrderBy(i => i.DisplayOrder).First()).ImageUrl
            : null,
        p.CreatedAt
        );
    }
}
