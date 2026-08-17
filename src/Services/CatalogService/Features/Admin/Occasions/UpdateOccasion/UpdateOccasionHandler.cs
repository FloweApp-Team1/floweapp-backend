using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Occasions.Common;
using MediatR;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;
namespace CatalogService.Features.Admin.Occasions.UpdateOccasion
{
    public class UpdateOccasionHandler : IRequestHandler<UpdateOccasionCommand, Result<OccasionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public UpdateOccasionHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IFileStorageService fileStorage,
            IIntegrationEventPublisher eventPublisher,
            IContentChangeLogger changeLogger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _fileStorage = fileStorage;
            _eventPublisher = eventPublisher;
            _changeLogger = changeLogger;
        }

        public async Task<Result<OccasionDto>> Handle(UpdateOccasionCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<Occasion>();
            var occasion = await repo.GetByIdAsync(request.OccasionId, cancellationToken);
            if (occasion is null)
                return Result.Failure<OccasionDto>(Error.New("Occasion.NotFound", "Occasion was not found."));

            if (request.Name is not null)
            {
                var nameTaken = await repo.ExistsAsync(o => o.Name == request.Name && o.Id != occasion.Id);
                if (nameTaken)
                    return Result.Failure<OccasionDto>(Error.New("Occasion.Conflict", "An occasion with this name already exists."));
                occasion.Name = request.Name;
            }

            if (request.Order.HasValue) occasion.DisplayOrder = request.Order.Value;

            if (request.Image is not null)
                occasion.ImageUrl = await _fileStorage.SaveAsync(request.Image, "occasions", cancellationToken);

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            occasion.UpdatedAt = DateTime.UtcNow;
            occasion.LastChangedBy = currentUserId;

            await _changeLogger.LogAsync("Occasion", occasion.Id, "Updated", currentUserId, $"Updated occasion '{occasion.Name}'.", cancellationToken);
            repo.Update(occasion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new OccasionUpdatedEvent(occasion.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(OccasionMapper.ToDto(occasion));
        }
    }
}
