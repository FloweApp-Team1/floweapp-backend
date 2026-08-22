using AddressCartService.Domain.Entities;
using AddressCartService.Domain.Enums;
using AddressCartService.Features.StoreCoverage.Common.Dtos;
using AddressCartService.Features.StoreCoverage.Common.Mapping;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Events.StoreEvents;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.DeactivateStore
{
    public class DeactivateStoreCommandHandler : IRequestHandler<DeactivateStoreCommand, Result<StoreResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IEventPublisher _eventPublisher;

        public DeactivateStoreCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IEventPublisher eventPublisher
            )
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _eventPublisher = eventPublisher;
        }

        public async Task<Result<StoreResponse>> Handle(DeactivateStoreCommand request, CancellationToken cancellationToken)
        {
            var storeRepository = _unitOfWork.Repository<Store>();
            var store = await storeRepository.GetByIdAsync(request.StoreId, cancellationToken);
            if (store is null)
                return Result.Failure<StoreResponse>(Error.New("Store.NotFound", "Store was not found."));

            if (store.Status == StoreStatusEnum.Inactive)
                return Result.Success(CoverageAreaMapper.ToStoreResponse(store));

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                store.Status = StoreStatusEnum.Inactive;
                store.UpdatedAt = DateTime.UtcNow;
                store.LastChangedBy = _currentUser.UserId ?? Guid.Empty;
                storeRepository.Update(store);
                var addressRepository = _unitOfWork.Repository<Address>();
                var dependentAddresses = await addressRepository
                    .GetAll(a => a.StoreId == store.Id && a.IsServiceable)
                    .ToListAsync(cancellationToken);

                foreach (var address in dependentAddresses)
                {
                    address.IsServiceable = false;
                    address.UpdatedAt = DateTime.UtcNow;
                    addressRepository.Update(address);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
            await _eventPublisher.PublishAsync(
                "store.deactivated",
                new StoreDeactivatedEvent(store.Id, store.Name, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(CoverageAreaMapper.ToStoreResponse(store));
        }
    }
}
