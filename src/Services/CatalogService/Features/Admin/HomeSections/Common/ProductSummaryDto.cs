namespace CatalogService.Features.Admin.HomeSections.Common
{
    public record ProductSummaryDto(
       Guid Id,
       string Name,
       decimal Price,
       decimal DiscountedPrice,
       int? DiscountPercent,
       bool InStock,
       string? ImageUrl);
}
