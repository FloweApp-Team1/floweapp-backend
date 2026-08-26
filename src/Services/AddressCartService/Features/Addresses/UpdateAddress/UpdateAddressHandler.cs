using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Services.StoreCoverage;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            private readonly AddressCartService.Infrastructure.Repositories.ILocationRepository _locationRepository;

            public UpdateAddressHandler(
                IUnitOfWork unitOfWork,
                ICurrentUserService currentUserService,
                IStoreResolutionService storeResolutionService,
                AddressCartService.Infrastructure.Repositories.ILocationRepository locationRepository)
            {
                _unitOfWork = unitOfWork;
                _currentUserService = currentUserService;
                _storeResolutionService = storeResolutionService;
                _locationRepository = locationRepository;
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
                    address.GovernorateId != request.GovernorateId ||
                    address.CityId != request.CityId ||
                    !string.Equals(address.Area, request.Area, StringComparison.Ordinal) ||
                    address.Lat != request.Lat ||
                    address.Lng != request.Lng;

                var city = await _locationRepository.GetCityWithGovernorateAsync(request.CityId, cancellationToken);
                
                if (city == null || city.GovernorateId != request.GovernorateId)
                {
                     return Result.Failure<UpdateAddressResponse>(
                        Error.New("Address.InvalidLocation", "Invalid Governorate or City."));
                }
                if (locationChanged)
                {
                var storeId = await _storeResolutionService.ResolveServingStoreAsync(
                    request.Lat, request.Lng, city.NameEn, request.Area, cancellationToken);

              
                if (storeId is null)
                {
                    return Result.Failure<UpdateAddressResponse>(
                        Error.New("Address.NotServiceable", "Cannot update address: The new location is outside our delivery coverage."));
                }

                address.StoreId = storeId;
                address.IsServiceable = true;
                }


            var now = DateTime.UtcNow;

                address.RecipientName = request.RecipientName;
                address.RecipientPhone = request.RecipientPhone;
                address.AddressLine = request.AddressLine;
                address.GovernorateId = request.GovernorateId;
                address.CityId = request.CityId;
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
                nameof(Address.GovernorateId),
                nameof(Address.CityId),
                nameof(Address.Area),
                nameof(Address.Label),
                nameof(Address.Lat),
                nameof(Address.Lng),
                nameof(Address.UpdatedAt),
                nameof(Address.LastChangedBy)
            };

                
                if (locationChanged)
                {
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
                    address.GovernorateId,
                    city.Governorate.NameEn,
                    address.CityId,
                    city.NameEn,
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
