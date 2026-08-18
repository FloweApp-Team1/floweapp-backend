using CatalogService.Domain.Entities;
using CatalogService.Features.Products.ListProducts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Models;
using Shared.Results;

namespace CatalogService.Features.Products.ListProducts
{
    public class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, Result<PagedResult<ListProductResponseDto>>>
    {
        private readonly IUnitOfWork unitOfWork;

        public ListProductsQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<Result<PagedResult<ListProductResponseDto>>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
        {
            var _repository = unitOfWork.Repository<Product>();
            var _productsQuery = _repository.Query().Where(e=>e.CategoryId==request.CategoryId).AsNoTracking();

            var totalCount = await _productsQuery.CountAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var products = await _productsQuery
                .OrderBy(e=>e.Price)
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(e => new
                {
                    product=e, 
                    ActiveDiscount= e.Discounts.FirstOrDefault(d => d.StartDate <= now && d.EndDate >= now)
                })
                .Select(p => new ListProductResponseDto
                {
                        Id = p.product.Id,
                        Name = p.product.Name,
                        OrignalPrice = p.product.Price,
                        DiscountPercentage = p.ActiveDiscount != null ? p.ActiveDiscount.Percentage : 0,

                        DiscountPrice = p.product.Price - (p.product.Price * (p.ActiveDiscount != null ? p.ActiveDiscount.Percentage : 0) / 100),

                        IsOutOfStock = p.product.StockQuantity <= 0,
                        ProductImages=p.product.ProductImages
                        .OrderBy(pi=>pi.DisplayOrder)
                        .Select(pi => new ProductImageDto   
                        {
                            Id = pi.Id,
                            ImageUrl = pi.ImageUrl,
                            DisplayOrder = pi.DisplayOrder,
                            IsPrimary= pi.IsPrimary      
                        }).ToList()

                }).ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<ListProductResponseDto>(products, totalCount);

            return Result<PagedResult<ListProductResponseDto>>.Success(pagedResult);


        }
    }
}
