using CatalogService.Domain.Entities;
using CatalogService.Features.Products;
using System.Linq.Expressions;

namespace CatalogService.Common
{
    public static class ProductMappingExtensions
    {
        
        public static readonly Expression<Func<Product, ProductListItemDto>> ToListItemDto = p => new ProductListItemDto(
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

        p.StockQuantity > 0,
        p.CategoryId,
        p.Category != null ? p.Category.Name : null,
   
        p.ProductImages != null && p.ProductImages.Any()
            ? (p.ProductImages.FirstOrDefault(i => i.IsPrimary) ?? p.ProductImages.OrderBy(i => i.DisplayOrder).First()).ImageUrl
            : null,
        p.CreatedAt
        );
    }
}
