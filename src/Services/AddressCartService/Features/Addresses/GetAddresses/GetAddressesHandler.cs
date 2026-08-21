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
                .GetAll(a => a.UserId == userId)
                .Select(x=>new AddressResponse
                (
                    x.Id,
                    x.RecipientName,
                    x.RecipientPhone,
                    x.AddressLine,
                    x.City,
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
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a=>a.CreatedAt)
                .ToListAsync(cancellationToken);

            return Result<List<AddressResponse>>.Success(addresses);

        }
    }
}
