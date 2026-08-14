using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Occasions.Common;
using MediatR;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Occasions.CreateOccasion
{
    public class CreateOccasionHandler : IRequestHandler<CreateOccasionCommand, Result<OccasionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public CreateOccasionHandler(
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

        public async Task<Result<OccasionDto>> Handle(CreateOccasionCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<Occasion>();

            var nameTaken = await repo.ExistsAsync(o => o.Name == request.Name);
            if (nameTaken)
                return Result.Failure<OccasionDto>(Error.New("Occasion.Conflict", "An occasion with this name already exists."));

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            string? imageUrl = null;
            if (request.Image is not null)
                imageUrl = await _fileStorage.SaveAsync(request.Image, "occasions", cancellationToken);

            var occasion = new Occasion
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ImageUrl = imageUrl,
                DisplayOrder = request.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedBy = currentUserId
            };

            await _changeLogger.LogAsync("Occasion", occasion.Id, "Created", currentUserId, $"Created occasion '{occasion.Name}'.", cancellationToken);
            await repo.AddAsync(occasion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new OccasionCreatedEvent(occasion.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(OccasionMapper.ToDto(occasion));
        }
    }
}
