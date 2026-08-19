using CatalogService.Domain.Entities;
using CatalogService.Features.Categories.ListCategories.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Responses;

namespace CatalogService.Features.Categories.ListCategories
{
    public class ListCategoriesQueryHandler : IRequestHandler<ListCategoriesQuery, ApiResponse<IReadOnlyList<ListCategoriesResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListCategoriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IReadOnlyList<ListCategoriesResponse>>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken)
        {
            var pagination = request.Pagination;

            var query = _unitOfWork.Repository<Category>()
                .Query()
                .AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .Select(c => new ListCategoriesResponse(
                    c.Id,
                    c.Name,
                    c.IconUrl,
                    c.DisplayOrder,
                    c.IsDeleted,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.LastChangedBy
                ))
                .ToListAsync(cancellationToken);

            return ApiResponse.Paginated(
                categories,
                totalCount,
                pagination,
                "Categories retrieved");
        }
    }
}
