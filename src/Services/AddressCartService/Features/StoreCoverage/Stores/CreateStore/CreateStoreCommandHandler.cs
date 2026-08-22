using AddressCartService.Domain.Entities;
using AddressCartService.Domain.Enums;
using AddressCartService.Features.StoreCoverage.Common.Dtos;
using AddressCartService.Features.StoreCoverage.Common.Mapping;
using MediatR;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.CreateStore
{
    public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, Result<StoreResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreateStoreCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<StoreResponse>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
        {
            var req = request.Request;
            var now = DateTime.UtcNow;

            var store = new Store
            {
                Id = Guid.NewGuid(),
                Name = req.Name.Trim(),
                Location = new StoreLocation
                {
                    AddressLine = req.Location.AddressLine.Trim(),
                    Lat = req.Location.Lat,
                    Lng = req.Location.Lng
                },
                CoverageArea = CoverageAreaMapper.ToDomain(req.CoverageArea),
                Status = StoreStatusEnum.Active,
                CreatedAt = now,
                UpdatedAt = now,
                LastChangedBy = _currentUser.UserId ?? Guid.Empty
            };

            var repository = _unitOfWork.Repository<Store>();
            await repository.AddAsync(store, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(CoverageAreaMapper.ToStoreResponse(store));
        }
    }
}
