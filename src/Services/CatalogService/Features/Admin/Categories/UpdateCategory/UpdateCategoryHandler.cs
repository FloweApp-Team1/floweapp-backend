using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Categories.Common;
using MediatR;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Categories.UpdateCategory
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public UpdateCategoryHandler(
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

        public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<Category>();
            var category = await repo.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
                return Result.Failure<CategoryDto>(Error.New("Category.NotFound", "Category was not found."));

            if (request.Name is not null)
            {
                var nameTaken = await repo.ExistsAsync(c => c.Name == request.Name && c.Id != category.Id);
                if (nameTaken)
                    return Result.Failure<CategoryDto>(Error.New("Category.Conflict", "A category with this name already exists."));
                category.Name = request.Name;
            }

            if (request.Order.HasValue) category.DisplayOrder = request.Order.Value;

            if (request.Icon is not null)
                category.IconUrl = await _fileStorage.SaveAsync(request.Icon, "categories", cancellationToken);

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            category.UpdatedAt = DateTime.UtcNow;
            category.LastChangedBy = currentUserId;

            await _changeLogger.LogAsync("Category", category.Id, "Updated", currentUserId, $"Updated category '{category.Name}'.", cancellationToken);
            repo.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new CategoryUpdatedEvent(category.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(CategoryMapper.ToDto(category));
        }
    }
}
