using CatalogService.Common;
using CatalogService.Common.Sorting;
using CatalogService.Domain.Entities;
using CatalogService.Features.Categories.ListCategories.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Models;
using Shared.Requests;
using Shared.Responses;
using Shared.Results;

namespace CatalogService.Features.Products
{
    public sealed record GetProductsQuery(
       Guid? CategoryId,
       Guid? OccasionId,
       Guid? StoreId,
       ProductSortOption? Sort,
       PaginationRequest Pagination) : IRequest<ApiResponse<IReadOnlyList<ProductListItemDto>>>;

    public sealed class GetProductsHandler(IGenericRepository<Product> productRepository)
        : IRequestHandler<GetProductsQuery, ApiResponse<IReadOnlyList<ProductListItemDto>>>
    {
        public async Task<ApiResponse<IReadOnlyList<ProductListItemDto>>> Handle(
         GetProductsQuery request, CancellationToken cancellationToken)
        {
            var query = productRepository.Query().AsNoTracking().ApplyFilters(request.CategoryId, request.OccasionId);


            var totalCount = await query.CountAsync(cancellationToken);



            var items = await query
                .ApplySort(request.Sort)
                .ApplyPagination(request.Pagination)
                .Select(ProductMappingExtensions.ToListItemDto(request.StoreId))
                .ToListAsync(cancellationToken);

            return ApiResponse.Paginated(
                items,
                totalCount,
                request.Pagination,
                "Products retrieved successfully");

        } 
    }
}
