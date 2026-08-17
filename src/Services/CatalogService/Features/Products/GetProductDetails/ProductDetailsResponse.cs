namespace CatalogService.Features.Products.GetProductDetails
{
    public sealed record GetProductDetailsResponse(
    Guid Id,
    string Name,
    decimal Price,
    decimal? DiscountedPrice,
    decimal? DiscountPercent,
    bool InStock,
    IReadOnlyList<string> Images,
    string Description,
    IReadOnlyList<string> Includes,
    string AvailabilityStatus,
    int AvailableStock,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<Guid> OccasionIds,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid LastChangedBy);
}