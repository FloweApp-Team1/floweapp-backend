using AddressCartService.Domain.Entities;
using AddressCartService.Features.StoreCoverage.Common.Dtos;
using AddressCartService.Features.StoreCoverage.Common.Mapping;
using MediatR;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.GetStore
{
    public class GetStoreQueryHandler : IRequestHandler<GetStoreQuery, Result<StoreResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStoreQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<StoreResponse>> Handle(GetStoreQuery request, CancellationToken cancellationToken)
        {
            var store = await _unitOfWork.Repository<Store>().GetByIdAsync(request.StoreId, cancellationToken);
            if (store is null)
                return Result.Failure<StoreResponse>(Error.New("Store.NotFound", "Store was not found."));

            return Result.Success(CoverageAreaMapper.ToStoreResponse(store));
        }
    }
}
