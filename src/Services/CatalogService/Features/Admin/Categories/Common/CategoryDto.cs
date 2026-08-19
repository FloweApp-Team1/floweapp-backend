namespace CatalogService.Features.Admin.Categories.Common
{
    public record CategoryDto(
         Guid Id,
         string Name,
         string? IconUrl,
         int Order,
         string Status,
         DateTime CreatedAt,
         DateTime UpdatedAt,
         Guid LastChangedBy);
}
