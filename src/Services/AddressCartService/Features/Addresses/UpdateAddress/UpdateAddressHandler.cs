using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Services.StoreCoverage;
using MediatR;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Addresses.UpdateAddress
{
   
        public sealed class UpdateAddressHandler
            : IRequestHandler<UpdateAddressCommand, Result<UpdateAddressResponse>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ICurrentUserService _currentUserService;
            private readonly IStoreResolutionService _storeResolutionService;

            public UpdateAddressHandler(
                IUnitOfWork unitOfWork,
                ICurrentUserService currentUserService,
                IStoreResolutionService storeResolutionService)
            {
                _unitOfWork = unitOfWork;
                _currentUserService = currentUserService;
                _storeResolutionService = storeResolutionService;
            }

            public async Task<Result<UpdateAddressResponse>> Handle(
                UpdateAddressCommand request,
                CancellationToken cancellationToken)
            {
                var userId = _currentUserService.UserId;
                if (userId is null)
                {
                    return Result.Failure<UpdateAddressResponse>(
                        Error.New("Address.Unauthorized", "User is not authenticated."));
                }

                var addressRepository = _unitOfWork.Repository<Address>();

                var address = await addressRepository.FirstOrDefaultAsync(
                    a => a.Id == request.AddressId && a.UserId == userId.Value,
                    cancellationToken);

                if (address is null)
                {
                    return Result.Failure<UpdateAddressResponse>(
                        Error.New("Address.NotFound", "Address not found."));
                }

                var locationChanged =
                    !string.Equals(address.AddressLine, request.AddressLine, StringComparison.Ordinal) ||
                    !string.Equals(address.City, request.City, StringComparison.Ordinal) ||
                    !string.Equals(address.Area, request.Area, StringComparison.Ordinal) ||
                    address.Lat != request.Lat ||
                    address.Lng != request.Lng;

                var now = DateTime.UtcNow;

                address.RecipientName = request.RecipientName;
                address.RecipientPhone = request.RecipientPhone;
                address.AddressLine = request.AddressLine;
                address.City = request.City;
                address.Area = request.Area;
                address.Label = request.Label;
                address.Lat = request.Lat;
                address.Lng = request.Lng;
                address.UpdatedAt = now;
                address.LastChangedBy = userId.Value;

                var includedProperties = new List<string>
            {
                nameof(Address.RecipientName),
                nameof(Address.RecipientPhone),
                nameof(Address.AddressLine),
                nameof(Address.City),
                nameof(Address.Area),
                nameof(Address.Label),
                nameof(Address.Lat),
                nameof(Address.Lng),
                nameof(Address.UpdatedAt),
                nameof(Address.LastChangedBy)
            };

                // Re-resolve 
                if (locationChanged)
                {
                    var storeId = await _storeResolutionService.ResolveServingStoreAsync(
                        address.Lat, address.Lng, address.City, address.Area, cancellationToken);

                    address.StoreId = storeId;
                    address.IsServiceable = storeId.HasValue;

                    includedProperties.Add(nameof(Address.StoreId));
                    includedProperties.Add(nameof(Address.IsServiceable));
                }

                addressRepository.SaveInclude(address, includedProperties.ToArray());

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(new UpdateAddressResponse(
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
                    address.IsServiceable,
                    address.StoreId,
                    now));
            }
        }
}
