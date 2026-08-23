using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Services.StoreCoverage;
using MediatR;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Addresses.CreateAddress
{
    public class CreateAddressHandler : IRequestHandler<CreateAddressCommand, Result<CreateAddressResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IStoreResolutionService _storeResolution;

        public CreateAddressHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IStoreResolutionService storeResolution)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _storeResolution = storeResolution;
        }

        public async Task<Result<CreateAddressResponse>> Handle(
            CreateAddressCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId)
                return Result.Failure<CreateAddressResponse>(
                    Error.New("Address.Unauthorized", "User is not authenticated."));

            var repo = _unitOfWork.Repository<Address>();

            // First address for this user becomes the default automatically.
            var isFirstAddress = !await repo.ExistsAsync(a => a.UserId == userId);

            // Unresolved coverage doesn't block creation - it just leaves StoreId null
            // and IsServiceable false, per SCRUM-91.
            var storeId = await _storeResolution.ResolveServingStoreAsync(
                request.Lat, request.Lng, request.City, request.Area, cancellationToken);

            var now = DateTime.UtcNow;

            var address = new Address
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                AddressLine = request.AddressLine,
                City = request.City,
                Area = request.Area,
                Label = request.Label,
                Lat = request.Lat,
                Lng = request.Lng,
                IsDefault = isFirstAddress,
                StoreId = storeId,
                IsServiceable = storeId is not null,
                CreatedAt = now,
                UpdatedAt = now,
                LastChangedBy = userId
            };

            await repo.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new CreateAddressResponse(
                address.Id,
                address.RecipientName,
                address.RecipientPhone,
                address.AddressLine,
                address.City,
                address.Area,
                address.Label,
                address.Lat,
                address.Lng,
                address.IsDefault,
                address.StoreId,
                address.IsServiceable,
                address.CreatedAt,
                address.UpdatedAt));
        }
    }
}
