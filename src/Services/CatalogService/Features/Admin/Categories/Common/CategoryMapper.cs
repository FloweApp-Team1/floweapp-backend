using CatalogService.Domain.Entities;

namespace CatalogService.Features.Admin.Categories.Common
{
    public static class CategoryMapper
    {
        public static CategoryDto ToDto(Category category) => new(
            category.Id,
            category.Name,
            category.IconUrl,
            category.DisplayOrder,
            category.IsDeleted ? "ARCHIVED" : "ACTIVE",
            category.CreatedAt,
            category.UpdatedAt,
            category.LastChangedBy);
    }
}
