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
        var product = await repository.Query()
     .AsNoTracking()
            .Where(x => x.Id == request.ProductId && !x.IsDeleted)
            .Select(x => new GetProductDetailsResponse(
                x.Id,
                x.Name,
                x.Price,

                x.DiscountPercentage.HasValue
                    ? x.Price - (x.Price * x.DiscountPercentage.Value / 100)
                    : x.Price,

                x.DiscountPercentage,

                x.StockQuantity > 0,

                x.ProductImages!
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .ToList(),

                x.Description,

                x.Includes!
                    .Select(i => i.Name)
                    .ToList(),

                x.StockQuantity > 0
                    ? "IN_STOCK"
                    : "OUT_OF_STOCK",

                x.StockQuantity,

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