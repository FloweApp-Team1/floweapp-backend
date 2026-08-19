using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Requests;

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
        public static IQueryable<Product> ApplyFilters(
           this IQueryable<Product> query,
           Guid? categoryId,
           Guid? occasionId)
        {
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (occasionId.HasValue)
            {
                query = query.Where(p => p.Occasions.Any(o => o.Id == occasionId.Value));
            }

            return query;
        }

        public static IQueryable<Product> ApplyPagination(
           this IQueryable<Product> query,
           PaginationRequest pagination)
        {
            return query
                .Skip(pagination.Skip)
                .Take(pagination.PageSize);
        }

        
    }
}
