using CatalogService.Common;
using CatalogService.Common.Sorting;
using CatalogService.Domain.Entities;
using MediatR;
using Shared.Interfaces;
using Shared.Requests;
using Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Features.Products.SearchProducts
{
    public sealed record SearchProductsQuery(
       string Q,
       ProductSortOption? Sort,
       PaginationRequest Pagination) : IRequest<ApiResponse<IReadOnlyList<ProductListItemDto>>>;

    public sealed class SearchProductsHandler(IGenericRepository<Product> productRepository)
       : IRequestHandler<SearchProductsQuery, ApiResponse<IReadOnlyList<ProductListItemDto>>>
    {
        public async Task<ApiResponse<IReadOnlyList<ProductListItemDto>>> Handle(
            SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var term = request.Q.Trim().ToLower();
            var query = productRepository.Query()
                .AsNoTracking()
                .Where(p => p.Name.ToLower().Contains(term));

            var totalCount = await query.CountAsync(cancellationToken);

            var ordered = request.Sort.HasValue

                ? query.ApplySort(request.Sort)

                : query
                    .OrderBy(p => EF.Functions.Like(p.Name, $"{term}%") ? 0 : 1)
                    .ThenBy(p => p.Name)
                    .ThenBy(p => p.Id);

            var items = await ordered
                .ApplyPagination(request.Pagination)
                .Select(ProductMappingExtensions.ToListItemDto)
                .ToListAsync(cancellationToken);

            return ApiResponse.Paginated(
                items,
                totalCount,
                request.Pagination,
                "Search results retrieved successfully");
        }
    }
}

