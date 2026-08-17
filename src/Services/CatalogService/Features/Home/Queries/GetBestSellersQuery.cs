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

        public GetBestSellersQuery(int count)
        {
            Count = count;
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
                .OrderByDescending(p => p.CreatedAt)
                .Take(request.Count)
                .Include(p => p.ProductImages)
                .ToListAsync(cancellationToken);

            var dtos = products.Select(p => new ProductItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ProductImages?.FirstOrDefault(img => img.IsPrimary)?.ImageUrl 
                           ?? p.ProductImages?.FirstOrDefault()?.ImageUrl 
                           ?? string.Empty
            }).ToList();

            return Result.Success(dtos);
        }
    }
}
