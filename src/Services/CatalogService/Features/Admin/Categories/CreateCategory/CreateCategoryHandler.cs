using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Categories.Common;
using MediatR;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Categories.CreateCategory
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public CreateCategoryHandler(
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

        public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<Category>();

            var nameTaken = await repo.ExistsAsync(c => c.Name == request.Name);
            if (nameTaken)
                return Result.Failure<CategoryDto>(Error.New("Category.Conflict", "A category with this name already exists."));

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            string? iconUrl = null;
            if (request.Icon is not null)
                iconUrl = await _fileStorage.SaveAsync(request.Icon, "categories", cancellationToken);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                IconUrl = iconUrl,
                DisplayOrder = request.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedBy = currentUserId
            };

            await _changeLogger.LogAsync("Category", category.Id, "Created", currentUserId, $"Created category '{category.Name}'.", cancellationToken);
            await repo.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new CategoryCreatedEvent(category.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(CategoryMapper.ToDto(category));
        }
    }
}
