using CatalogService.Domain.Entities;
using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace CatalogService.Features.Occasions.ListOccasions
{
    public class ListOccasionsHandler : IRequestHandler<ListOccasionsQuery, ApiResponse<PagedResult<ListOccasionsResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListOccasionsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResult<ListOccasionsResponse>>> Handle(
            ListOccasionsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Repository<Occasion>().GetAll(x=>!x.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            var occasions = await query
                .OrderBy(o => o.DisplayOrder)
                .Skip(request.Pagination.Skip)
                .Take(request.Pagination.PageSize)
                .Select(o => new ListOccasionsResponse(
                
                    o.Id,
                    o.Name,
                    o.ImageUrl,
                    o.DisplayOrder,
                    o.IsDeleted,
                    o.CreatedAt,
                    o.UpdatedAt,
                    o.LastChangedBy
                ))
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<ListOccasionsResponse>(occasions, totalCount);

            return ApiResponse<PagedResult<ListOccasionsResponse>>.Success(pagedResult);
        }
    }
}
