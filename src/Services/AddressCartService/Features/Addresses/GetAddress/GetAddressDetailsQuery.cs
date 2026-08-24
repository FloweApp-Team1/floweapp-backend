using AddressCartService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Addresses.GetAddress
{
    public sealed record GetAddressDetailsQuery(Guid AddressId)
         : IRequest<Result<AddressDetailsResponse>>;
    public sealed class GetAddressDetailsHandler
        : IRequestHandler<GetAddressDetailsQuery, Result<AddressDetailsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetAddressDetailsHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<AddressDetailsResponse>> Handle(
            GetAddressDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure<AddressDetailsResponse>(
                    Error.New("Address.Unauthorized", "User is not authenticated."));
            }
            var address = await _unitOfWork.Repository<Address>()
                .Query()
                .AsNoTracking()
                .Where(a => a.Id == request.AddressId && a.UserId == userId.Value)
                .Select(a => new AddressDetailsResponse(
                    a.Id,
                    a.RecipientName,
                    a.RecipientPhone,
                    a.AddressLine,
                    a.GovernorateId,
                    a.Governorate != null ? a.Governorate.NameEn : "",
                    a.CityId,
                    a.City != null ? a.City.NameEn : "",
                    a.Area,
                    a.Label,
                    a.Lat,
                    a.Lng,
                    a.IsDefault,
                    a.IsServiceable,
                    a.StoreId,
                    a.CreatedAt,
                    a.UpdatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            if (address is null)
            {
                return Result.Failure<AddressDetailsResponse>(
                    Error.New("Address.NotFound", "Address not found."));
            }

            return Result.Success(address);
        }
    }
}
