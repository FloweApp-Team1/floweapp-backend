using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.HomeSections.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.HomeSections.ToggleHomeSectionStatus
{
    public class ToggleHomeSectionStatusHandler : IRequestHandler<ToggleHomeSectionStatusCommand, Result<HomeSectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public ToggleHomeSectionStatusHandler(
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

        public async Task<Result<HomeSectionDto>> Handle(ToggleHomeSectionStatusCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<HomeSection>();

            var section = await repo.Query()
                .Where(s => s.Id == request.SectionId)
                .Include(s => s.SectionCategories!).ThenInclude(sc => sc.Category)
                .Include(s => s.SectionOccasions!).ThenInclude(so => so.Occasion)
                .Include(s => s.SectionProducts!).ThenInclude(sp => sp.Product).ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(cancellationToken);

            if (section is null)
                return Result.Failure<HomeSectionDto>(Error.New("HomeSection.NotFound", "Home section was not found."));

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            section.Enabled = request.Enabled;
            section.UpdatedAt = DateTime.UtcNow;
            section.LastChangedBy = currentUserId;

            await _changeLogger.LogAsync("HomeSection", section.Id, request.Enabled ? "Enabled" : "Disabled", currentUserId, $"Section '{section.Title}' {(request.Enabled ? "enabled" : "disabled")}.", cancellationToken);
            repo.Update(section);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new HomeSectionsChangedEvent(currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(HomeSectionMapper.ToDto(section));
        }
    }
}
