using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Categories.Common;
using MediatR;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Categories.ArchiveCategory
{
    public class ArchiveCategoryHandler : IRequestHandler<ArchiveCategoryCommand, Result<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public ArchiveCategoryHandler(
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

        public async Task<Result<CategoryDto>> Handle(ArchiveCategoryCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<Category>();
            var category = await repo.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
                return Result.Failure<CategoryDto>(Error.New("Category.NotFound", "Category was not found."));

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            category.LastChangedBy = currentUserId;
            category.UpdatedAt = DateTime.UtcNow;

            await _changeLogger.LogAsync("Category", category.Id, "Archived", currentUserId, $"Archived category '{category.Name}'.", cancellationToken);
            repo.Remove(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new CategoryArchivedEvent(category.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);
            return Result.Success(CategoryMapper.ToDto(category));
        }
    }
}
