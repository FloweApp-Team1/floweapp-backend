using AddressCartService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Addresses.GetAddresses
{
    public class GetAddressesHandler : IRequestHandler<GetAddressesQuery, Result<List<AddressResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetAddressesHandler(IUnitOfWork unitOfWork , ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<List<AddressResponse>>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
        {
            if(_currentUser.UserId is not { } userId)
                return Result<List<AddressResponse>>.Failure(Error.New("Address.Unauthorized", "User is not authenticated."));

            var addresses = await _unitOfWork.Repository<Address>()
                .Query()
                .Include(a => a.Governorate)
                .Include(a => a.City)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .Select(x => new AddressResponse
                (
                    x.Id,
                    x.RecipientName,
                    x.RecipientPhone,
                    x.AddressLine,
                    x.GovernorateId,
                    x.Governorate != null ? x.Governorate.NameEn : "",
                    x.CityId,
                    x.City != null ? x.City.NameEn : "",
                    x.Area,
                    x.Label,
                    x.Lat,
                    x.Lng,
                    x.IsDefault,
                    x.StoreId,
                    x.IsServiceable,
                    x.CreatedAt,
                    x.UpdatedAt
                ))
                .ToListAsync(cancellationToken);

            return Result<List<AddressResponse>>.Success(addresses);

        }
    }
}
