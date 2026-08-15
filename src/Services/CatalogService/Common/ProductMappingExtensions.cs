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
            p.StockQuantity > 0,
            p.CategoryId,
            p.Category != null ? p.Category.Name : null,
            p.ProductImages != null && p.ProductImages.Any(i => i.IsPrimary)
                ? p.ProductImages.First(i => i.IsPrimary).ImageUrl
                : p.ProductImages != null && p.ProductImages.Any()
                    ? p.ProductImages.OrderBy(i => i.DisplayOrder).First().ImageUrl
                    : null,
            p.CreatedAt
        );
    }
}
