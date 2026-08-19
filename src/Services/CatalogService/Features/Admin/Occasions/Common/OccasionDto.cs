namespace CatalogService.Features.Admin.Occasions.Common
{
    public record OccasionDto(
         Guid Id,
         string Name,
         string? ImageUrl,
         int Order,
         string Status,
         DateTime CreatedAt,
         DateTime UpdatedAt,
         Guid LastChangedBy);
}
