using AddressCartService.Domain.Entities;
using AddressCartService.Features.StoreCoverage.Common.Mapping;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.GetStores
{
    public class GetStoresQueryHandler : IRequestHandler<GetStoresQuery, Result<PagedStoresResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStoresQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedStoresResult>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var query = _unitOfWork.Repository<Store>().Query().OrderBy(s => s.Name);

            var totalCount = await query.CountAsync(cancellationToken);
            var stores = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = stores.Select(CoverageAreaMapper.ToStoreResponse).ToList();
            return Result.Success(new PagedStoresResult(items, totalCount));
        }
    }
}
