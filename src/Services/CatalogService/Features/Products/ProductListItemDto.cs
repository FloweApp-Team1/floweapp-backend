namespace CatalogService.Features.Products
{
    public sealed record ProductListItemDto(
        Guid Id,
        string Name,
        decimal Price,
        bool InStock,
        Guid CategoryId,
        string? CategoryName,
        string? ImageUrl,
        DateTime CreatedAt);
}
