using CatalogService.Domain.Entities;
using CatalogService.Features.Home.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Home.Queries
{
    public class GetTopOccasionsQuery : IRequest<Result<List<OccasionItemDto>>>
    {
        public int Count { get; }

        public GetTopOccasionsQuery(int count)
        {
            Count = count;
        }
    }

    public class GetTopOccasionsQueryHandler : IRequestHandler<GetTopOccasionsQuery, Result<List<OccasionItemDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTopOccasionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<OccasionItemDto>>> Handle(GetTopOccasionsQuery request, CancellationToken cancellationToken)
        {
            var occasions = await _unitOfWork.Repository<Occasion>()
                .GetAll()
                .OrderBy(o => o.DisplayOrder)
                .Take(request.Count)
                .ToListAsync(cancellationToken);

            var dtos = occasions.Select(o => new OccasionItemDto
            {
                Id = o.Id,
                Name = o.Name,
                ImageUrl = o.ImageUrl ?? string.Empty
            }).ToList();

            return Result.Success(dtos);
        }
    }
}
