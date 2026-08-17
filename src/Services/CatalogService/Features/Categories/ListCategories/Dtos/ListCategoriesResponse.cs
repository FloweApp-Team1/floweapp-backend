namespace CatalogService.Features.Categories.ListCategories.Dtos
{
    // Shape the category rail / Categories screen renders: icon + label, plus the
    // id the client passes to /products?categoryId=, followed by the audit fields.
    public record ListCategoriesResponse(
        Guid Id,
        string Name,
        string? IconUrl,
        int DisplayOrder,
        bool IsDeleted,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        Guid LastChangedBy
    );
}
