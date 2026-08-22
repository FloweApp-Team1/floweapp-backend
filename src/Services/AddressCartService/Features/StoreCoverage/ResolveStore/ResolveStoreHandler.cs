using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Services.StoreCoverage;
using MediatR;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.ResolveStore
{
    public class ResolveStoreHandler : IRequestHandler<ResolveStoreQuery, Result<ResolveStoreResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IStoreResolutionService _storeResolution;

        public ResolveStoreHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IStoreResolutionService storeResolution)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _storeResolution = storeResolution;
        }

        public async Task<Result<ResolveStoreResponse>> Handle(
            ResolveStoreQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId)
                return Result.Failure<ResolveStoreResponse>(
                    Error.New("Address.Unauthorized", "User is not authenticated."));

            if (request.AddressId is { } addressId)
                return await ResolveFromAddressAsync(addressId, userId, cancellationToken);

            // Lat/Lng path (e.g. detected current location that hasn't been saved as an
            // address yet) - city/area aren't known here, so only Polygon/Radius coverage
            // can match; CityAreaList stores simply can't be reached this way.
            var storeId = await _storeResolution.ResolveServingStoreAsync(
                request.Lat, request.Lng, city: string.Empty, area: string.Empty, cancellationToken);

            return Result.Success(new ResolveStoreResponse(storeId, storeId is not null));
        }

        private async Task<Result<ResolveStoreResponse>> ResolveFromAddressAsync(
            Guid addressId, Guid userId, CancellationToken cancellationToken)
        {
            // Addresses are resolved once at creation time (CreateAddressHandler) and the
            // result persisted on the row - re-use it instead of recomputing on every lookup.
            var address = await _unitOfWork.Repository<Address>()
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

            if (address is null)
                return Result.Failure<ResolveStoreResponse>(
                    Error.New("Address.NotFound", "Address not found."));

            return Result.Success(new ResolveStoreResponse(address.StoreId, address.IsServiceable));
        }
    }
}
