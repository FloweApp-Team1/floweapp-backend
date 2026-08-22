using AddressCartService.Domain.Entities;
using AddressCartService.Features.StoreCoverage.Common.Dtos;
using AddressCartService.Features.StoreCoverage.Common.Mapping;
using MediatR;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.UpdateStore
{
    public class UpdateStoreCommandHandler : IRequestHandler<UpdateStoreCommand, Result<StoreResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateStoreCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<StoreResponse>> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Store>();
            var store = await repository.GetByIdAsync(request.StoreId, cancellationToken);
            if (store is null)
                return Result.Failure<StoreResponse>(Error.New("Store.NotFound", "Store was not found."));

            var req = request.Request;

            store.Name = req.Name.Trim();
            store.Location.AddressLine = req.Location.AddressLine.Trim();
            store.Location.Lat = req.Location.Lat;
            store.Location.Lng = req.Location.Lng;
            CoverageAreaMapper.ApplyTo(store.CoverageArea, req.CoverageArea);

            store.UpdatedAt = DateTime.UtcNow;
            store.LastChangedBy = _currentUser.UserId ?? Guid.Empty;

            repository.Update(store);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(CoverageAreaMapper.ToStoreResponse(store));
        }
    }
}
