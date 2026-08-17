using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Admin.Products.CreateProduct
{
    public record CreateProductCommand(
        string Name,
        string Description,
        List<string> Includes,
        decimal Price,
        int? DiscountPercent,
        List<Guid> CategoryIds,
        List<Guid> OccasionIds,
        List<IFormFile> Images,
        List<StoreStockItem> StoreStock) : IRequest<Result<ProductDto>>;

}
