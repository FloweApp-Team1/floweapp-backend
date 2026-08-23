using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Products.GetProductDetails;

public sealed class GetProductDetailsQueryHandler(
    IGenericRepository<Product> repository)
    : IRequestHandler<GetProductDetailsQuery, Result<GetProductDetailsResponse>>
{
    public async Task<Result<GetProductDetailsResponse>> Handle(
        GetProductDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var storeId = request.StoreId;

        var product = await repository.Query()
            .AsNoTracking()
            .Where(x => x.Id == request.ProductId )
            .Select(x => new GetProductDetailsResponse(
                x.Id,
                x.Name,
                x.Price,

             x.Discounts
                    .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                    .OrderByDescending(d => d.StartDate)
                    .Select(d => (decimal?)(x.Price - (x.Price * d.Percentage / 100)))
                    .FirstOrDefault(),
 
                x.Discounts
                    .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                    .OrderByDescending(d => d.StartDate)
                    .Select(d => (decimal?)d.Percentage)
                    .FirstOrDefault(),

                storeId == null
                    ? x.StockQuantity > 0
                    : x.StoreStocks!.Any(s => s.StoreId == storeId && s.Quantity > 0),

                x.ProductImages!
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .ToList(),

                x.Description,

                x.Includes!
                    .Select(i => i.Name)
                    .ToList(),

                (storeId == null
                    ? x.StockQuantity
                    : x.StoreStocks!.Where(s => s.StoreId == storeId).Select(s => s.Quantity).FirstOrDefault()) > 0
                    ? "IN_STOCK"
                    : "OUT_OF_STOCK",

                storeId == null
                    ? x.StockQuantity
                    : x.StoreStocks!.Where(s => s.StoreId == storeId).Select(s => s.Quantity).FirstOrDefault(),

                new List<Guid>
                {
                   x.CategoryId
                },

                x.ProductOccasions!
                    .Select(po => po.OccasionId)
                    .ToList(),

                x.CreatedAt,
                x.UpdatedAt,
                x.LastChangedBy
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result<GetProductDetailsResponse>
                .Failure("Product not found");
        }

        return Result<GetProductDetailsResponse>
            .Success(product);
    }
}