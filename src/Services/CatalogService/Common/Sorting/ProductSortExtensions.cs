using CatalogService.Domain.Entities;

namespace CatalogService.Common.Sorting
{
    public static class ProductSortExtensions
    {
        public static IQueryable<Product> ApplySort(this IQueryable<Product> query, ProductSortOption? sort)
        {
            return sort switch
            {
                ProductSortOption.PriceLowToHigh => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
                ProductSortOption.PriceHighToLow => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
                ProductSortOption.OldestFirst => query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
                ProductSortOption.NewestFirst or null => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id),
                // Default order (no sort selected) is Newest First
                _ => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
            };
        }
    }
}
