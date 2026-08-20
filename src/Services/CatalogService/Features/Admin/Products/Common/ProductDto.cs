namespace CatalogService.Features.Admin.Products.Common
{
    public record ProductDto(
        Guid Id,
        string Name,
        decimal Price,
        decimal DiscountedPrice,
        int? DiscountPercent,
        bool InStock,
        List<string> Images,
        string Description,
        List<string> Includes,
        string AvailabilityStatus,
        int AvailableStock,
        List<Guid> CategoryIds,
        List<Guid> OccasionIds,
        string CatalogStatus,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        Guid LastChangedBy);
}
