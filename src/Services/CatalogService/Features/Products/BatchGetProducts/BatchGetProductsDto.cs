namespace CatalogService.Features.Products.BatchGetProducts
{
    public sealed record BatchProductItemDto(
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

    public sealed record BatchGetProductsResponse(IReadOnlyList<BatchProductItemDto> Products);

    public sealed record BatchGetProductsRequest(List<Guid> ProductIds, Guid? StoreId);
}
