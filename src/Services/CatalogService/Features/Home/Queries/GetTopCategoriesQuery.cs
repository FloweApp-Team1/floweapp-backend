using CatalogService.Domain.Entities;
using CatalogService.Features.Home.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Home.Queries
{
    public class GetTopCategoriesQuery : IRequest<Result<List<CategoryItemDto>>>
    {
        public int Count { get; }

        public GetTopCategoriesQuery(int count)
        {
            Count = count;
        }
    }

    public class GetTopCategoriesQueryHandler : IRequestHandler<GetTopCategoriesQuery, Result<List<CategoryItemDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTopCategoriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<CategoryItemDto>>> Handle(GetTopCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.Repository<Category>()
                .GetAll()
                .OrderBy(c => c.DisplayOrder)
                .Take(request.Count)
                .ToListAsync(cancellationToken);

            var dtos = categories.Select(c => new CategoryItemDto
            {
                Id = c.Id,
                Name = c.Name,
                IconUrl = c.IconUrl ?? string.Empty
            }).ToList();

            return Result.Success(dtos);
        }
    }
}
