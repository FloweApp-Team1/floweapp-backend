using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Addresses.DeleteAddress
{
    
    public sealed class DeleteAddressHandler : IRequestHandler<DeleteAddressCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteAddressHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result.Failure(Error.New("Address.Unauthorized", "User is not authenticated."));

            var addressrepository = _unitOfWork.Repository<Address>();

            var address = await addressrepository.FirstOrDefaultAsync(
                a => a.Id == request.AddressId && a.UserId == userId.Value,
                cancellationToken);

            if (address is null)
                return Result.Failure(Error.New("Address.NotFound", "Address not found."));

            var wasDefault = address.IsDefault;
            var now = DateTime.UtcNow;


            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                address.IsDefault = false;
                address.UpdatedAt = now;
                address.LastChangedBy = userId.Value;

                addressrepository.Remove(address);

                await _unitOfWork.SaveChangesAsync(cancellationToken);


                if (wasDefault)
                {

                    var nextDefault = await addressrepository.Query().AsNoTracking()
                        .Where(a => a.UserId == userId.Value && a.Id != address.Id)
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (nextDefault is not null)
                    {
                        nextDefault.IsDefault = true;
                        nextDefault.UpdatedAt = now;
                        nextDefault.LastChangedBy = userId.Value;

                        addressrepository.SaveInclude(nextDefault,
                             nameof(Address.IsDefault),
                             nameof(Address.UpdatedAt),
                             nameof(Address.LastChangedBy));
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                }
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return Result.Success();
                }
            
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(
                    Error.New("Address.Conflict",
                        "The default address was changed concurrently. Please retry."));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }



        }
    }
}
