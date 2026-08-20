using CatalogService.Domain.Entities;

namespace CatalogService.Features.Admin.Occasions.Common
{
    public static class OccasionMapper
    {
        public static OccasionDto ToDto(Occasion occasion) => new(
            occasion.Id,
            occasion.Name,
            occasion.ImageUrl,
            occasion.DisplayOrder,
            occasion.IsDeleted ? "ARCHIVED" : "ACTIVE",
            occasion.CreatedAt,
            occasion.UpdatedAt,
            occasion.LastChangedBy);
    }
}
