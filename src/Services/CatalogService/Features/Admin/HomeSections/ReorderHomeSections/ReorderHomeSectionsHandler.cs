using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.HomeSections.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.HomeSections.ReorderHomeSections
{
    public class ReorderHomeSectionsHandler : IRequestHandler<ReorderHomeSectionsCommand, Result<List<HomeSectionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public ReorderHomeSectionsHandler(
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

        public async Task<Result<List<HomeSectionDto>>> Handle(ReorderHomeSectionsCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<HomeSection>();
            var ids = request.Sections.Select(s => s.Id).ToList();

            var sections = await repo.Query()
                .Where(s => ids.Contains(s.Id))
                .ToListAsync(cancellationToken);

            if (sections.Count != ids.Distinct().Count())
                return Result.Failure<List<HomeSectionDto>>(Error.New("HomeSection.NotFound", "One or more Home sections were not found."));

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            var orderById = request.Sections.ToDictionary(s => s.Id, s => s.Order);

            foreach (var section in sections)
            {
                section.Order = orderById[section.Id];
                section.UpdatedAt = DateTime.UtcNow;
                section.LastChangedBy = currentUserId;
                repo.Update(section);

                await _changeLogger.LogAsync(
                    "HomeSection", section.Id, "Reordered", currentUserId,
                    $"Moved '{section.Title}' to order {section.Order}.", cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await repo.Query()
                .Where(s => ids.Contains(s.Id))
                .OrderBy(s => s.Order)
                .Include(s => s.SectionCategories!).ThenInclude(sc => sc.Category)
                .Include(s => s.SectionOccasions!).ThenInclude(so => so.Occasion)
                .Include(s => s.SectionProducts!).ThenInclude(sp => sp.Product).ThenInclude(p => p.ProductImages)
                .ToListAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new HomeSectionsChangedEvent(currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(updated.Select(HomeSectionMapper.ToDto).ToList());
        }
    }
}
