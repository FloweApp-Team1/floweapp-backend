using CatalogService.Domain.Entities;
using Shared.Interfaces;

namespace CatalogService.Infrastructure.Services
{
    public class ContentChangeLogger : IContentChangeLogger
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContentChangeLogger(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(
            string entityType,
            Guid entityId,
            string action,
            Guid changedBy,
            string? summary = null,
            CancellationToken cancellationToken = default)
        {
            var entry = new ContentChangeLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                Summary = summary
            };

            await _unitOfWork.Repository<ContentChangeLog>().AddAsync(entry, cancellationToken);
        }
    }
}
