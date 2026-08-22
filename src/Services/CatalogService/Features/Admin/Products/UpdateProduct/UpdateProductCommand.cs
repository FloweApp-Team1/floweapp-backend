using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Results;

namespace CatalogService.Features.Admin.Products.UpdateProduct
{
    public record UpdateProductCommand(
     Guid ProductId,
     string? Name,
     string? Description,
     List<string>? Includes,
     decimal? Price,
     int? DiscountPercent,
     List<Guid>? CategoryIds,
     List<Guid>? OccasionIds,
     List<IFormFile>? Images,
     List<StoreStockItem>? StoreStock) : IRequest<Result<ProductDto>>;
}
