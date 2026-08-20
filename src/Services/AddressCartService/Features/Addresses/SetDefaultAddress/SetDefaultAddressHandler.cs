using AddressCartService.Domain.Entities;
using MediatR;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Addresses.SetDefaultAddress
{

    public sealed class SetDefaultAddressHandler
        : IRequestHandler<SetDefaultAddressCommand, Result<SetDefaultAddressResponse>>
    {
      

        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public SetDefaultAddressHandler(
          
           ICurrentUserService currentUserService,IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;           
        }

        public async Task<Result<SetDefaultAddressResponse>> Handle(
            SetDefaultAddressCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure<SetDefaultAddressResponse>(
                    Error.New("Address.Unauthorized", "User is not authenticated."));
            }
            var _addressRepository = _unitOfWork.Repository<Address>();

            var targetAddress = await _addressRepository.FirstOrDefaultAsync(
                a => a.Id == request.AddressId && a.UserId == userId.Value,
                cancellationToken);

            if (targetAddress is null)
            {
                return Result.Failure<SetDefaultAddressResponse>(
                    Error.New("Address.NotFound", "Address not found."));
            }
            if (targetAddress.IsDefault)
            {
                return Result.Success(new SetDefaultAddressResponse(
                    targetAddress.Id, true, targetAddress.UpdatedAt));
            }

            var previousDefault = await _addressRepository.FirstOrDefaultAsync(
                a => a.UserId == userId.Value && a.IsDefault,
                cancellationToken);

            var now = DateTime.UtcNow;

            if (previousDefault is not null)
            {
                previousDefault.IsDefault = false;
                previousDefault.UpdatedAt = now;
                previousDefault.LastChangedBy = userId.Value;

                _addressRepository.SaveInclude(
                    previousDefault,
                    nameof(Address.IsDefault),
                    nameof(Address.UpdatedAt),
                    nameof(Address.LastChangedBy));
            }


            targetAddress.IsDefault = true;
            targetAddress.UpdatedAt = now;
            targetAddress.LastChangedBy = userId.Value;

            _addressRepository.SaveInclude(
                targetAddress,
                nameof(Address.IsDefault),
                nameof(Address.UpdatedAt),
                nameof(Address.LastChangedBy));

           
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new SetDefaultAddressResponse(targetAddress.Id, true, now));
        }
    }
}
