using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Products.BatchGetProducts
{
    public sealed class BatchGetProductsQueryHandler(IGenericRepository<Product> repository)
        : IRequestHandler<BatchGetProductsQuery, Result<BatchGetProductsResponse>>
    {
        public async Task<Result<BatchGetProductsResponse>> Handle(
            BatchGetProductsQuery request, CancellationToken cancellationToken)
        {
            if (request.ProductIds == null || request.ProductIds.Count == 0)
            {
                return Result.Success(new BatchGetProductsResponse(new List<BatchProductItemDto>()));
            }

            var storeId = request.StoreId;
            var now = DateTime.UtcNow;

            var products = await repository.Query()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => request.ProductIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.IsDeleted,
                    DiscountedPrice = p.Discounts
                        .Where(d => d.StartDate <= now && d.EndDate >= now)
                        .OrderByDescending(d => d.StartDate)
                        .Select(d => (decimal?)(p.Price - (p.Price * d.Percentage / 100)))
                        .FirstOrDefault(),
                    DiscountPercent = p.Discounts
                        .Where(d => d.StartDate <= now && d.EndDate >= now)
                        .OrderByDescending(d => d.StartDate)
                        .Select(d => (decimal?)d.Percentage)
                        .FirstOrDefault(),
                    PrimaryImage = p.ProductImages!
                        .OrderByDescending(i => i.IsPrimary)
                        .ThenBy(i => i.DisplayOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    AvailableStock = storeId == null
                        ? p.StockQuantity
                        : p.StoreStocks!.Where(s => s.StoreId == storeId).Select(s => s.Quantity).FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var dtos = products.Select(p =>
            {
                var effectivePrice = p.DiscountedPrice ?? p.Price;
                var isAvailable = !p.IsDeleted && p.AvailableStock > 0;
                return new BatchProductItemDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    effectivePrice,
                    p.DiscountedPrice,
                    p.DiscountPercent,
                    isAvailable,
                    p.AvailableStock,
                    p.PrimaryImage,
                    p.IsDeleted);
            }).ToList();

            return Result.Success(new BatchGetProductsResponse(dtos));
        }
    }
}
