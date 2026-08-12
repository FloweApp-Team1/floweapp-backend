using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Home.Queries
{
    public class GetHomeLayoutSectionsQuery : IRequest<Result<List<HomeLayoutSection>>>
    {
    }

    public class GetHomeLayoutSectionsQueryHandler : IRequestHandler<GetHomeLayoutSectionsQuery, Result<List<HomeLayoutSection>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetHomeLayoutSectionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<HomeLayoutSection>>> Handle(GetHomeLayoutSectionsQuery request, CancellationToken cancellationToken)
        {
            var sections = await _unitOfWork.Repository<HomeLayoutSection>()
                .GetAll(x => x.isEnabled)
                .OrderBy(x => x.order)
                .ToListAsync(cancellationToken);

            return Result.Success(sections);
        }
    }
}
