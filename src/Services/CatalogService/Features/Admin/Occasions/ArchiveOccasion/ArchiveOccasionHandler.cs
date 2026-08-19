using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Occasions.Common;
using MediatR;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Occasions.ArchiveOccasion
{
    public class ArchiveOccasionHandler : IRequestHandler<ArchiveOccasionCommand, Result<OccasionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public ArchiveOccasionHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IIntegrationEventPublisher eventPublisher,
            IContentChangeLogger changeLogger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _eventPublisher = eventPublisher;
            _changeLogger = changeLogger;
        }

        public async Task<Result<OccasionDto>> Handle(ArchiveOccasionCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<Occasion>();
            var occasion = await repo.GetByIdAsync(request.OccasionId, cancellationToken);
            if (occasion is null)
                return Result.Failure<OccasionDto>(Error.New("Occasion.NotFound", "Occasion was not found."));

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            occasion.LastChangedBy = currentUserId;
            occasion.UpdatedAt = DateTime.UtcNow;

            await _changeLogger.LogAsync("Occasion", occasion.Id, "Archived", currentUserId, $"Archived occasion '{occasion.Name}'.", cancellationToken);
            repo.Remove(occasion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new OccasionArchivedEvent(occasion.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(OccasionMapper.ToDto(occasion));
        }
    }
}
