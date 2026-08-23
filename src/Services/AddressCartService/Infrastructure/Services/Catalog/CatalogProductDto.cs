namespace AddressCartService.Infrastructure.Services.Catalog
{
    public sealed record CatalogProductDto(
        Guid Id,
        string Name,
        decimal Price,
        decimal EffectivePrice,
        decimal? DiscountedPrice,
        decimal? DiscountPercent,
        bool IsAvailable,
        int AvailableStock,
        string? PrimaryImageUrl,
        bool IsArchived);
}
