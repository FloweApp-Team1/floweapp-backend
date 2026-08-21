using CatalogService.Domain.Entities;
using CatalogService.Features.Home.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Home.Queries
{
    public class GetBestSellersQuery : IRequest<Result<List<ProductItemDto>>>
    {
        public int Count { get; }
        public Guid? StoreId { get; }

        public GetBestSellersQuery(int count, Guid? storeId = null)
        {
            Count = count;
            StoreId = storeId;
        }
    }

    public class GetBestSellersQueryHandler : IRequestHandler<GetBestSellersQuery, Result<List<ProductItemDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBestSellersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ProductItemDto>>> Handle(GetBestSellersQuery request, CancellationToken cancellationToken)
        {
            var products = await _unitOfWork.Repository<Product>()
                .GetAll()
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(request.Count)
                .Include(p => p.ProductImages)
                .Include(p => p.StoreStocks)
                .ToListAsync(cancellationToken);

            var storeId = request.StoreId;

            var dtos = products.Select(p => new ProductItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ProductImages?.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                           ?? p.ProductImages?.FirstOrDefault()?.ImageUrl
                           ?? string.Empty,
                InStock = storeId == null
                    ? p.StockQuantity > 0
                    : p.StoreStocks?.Any(s => s.StoreId == storeId && s.Quantity > 0) ?? false
            }).ToList();

            return Result.Success(dtos);
        }
    }
}
