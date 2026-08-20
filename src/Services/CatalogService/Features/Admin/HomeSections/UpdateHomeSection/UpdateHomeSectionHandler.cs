using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.HomeSections.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.HomeSections.UpdateHomeSection
{
    public class UpdateHomeSectionHandler : IRequestHandler<UpdateHomeSectionCommand, Result<HomeSectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public UpdateHomeSectionHandler(
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

        public async Task<Result<HomeSectionDto>> Handle(UpdateHomeSectionCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<HomeSection>();

            var section = await repo.Query()
                .Where(s => s.Id == request.SectionId)
                .Include(s => s.SectionCategories!)
                .Include(s => s.SectionOccasions!)
                .Include(s => s.SectionProducts!)
                .FirstOrDefaultAsync(cancellationToken);

            if (section is null)
                return Result.Failure<HomeSectionDto>(Error.New("HomeSection.NotFound", "Home section was not found."));

            if (request.Title is not null) section.Title = request.Title;
            if (request.Order.HasValue) section.Order = request.Order.Value;
            if (request.Enabled.HasValue) section.Enabled = request.Enabled.Value;
            if (request.ViewAllDeeplink is not null) section.ViewAllDeeplink = request.ViewAllDeeplink;
            if (request.BannerImageUrl is not null) section.BannerImageUrl = request.BannerImageUrl;
            if (request.BannerDeeplink is not null) section.BannerDeeplink = request.BannerDeeplink;
            if (request.ProductSelectionRule is not null)
                section.ProductSelectionRule = ProductSelectionRuleParser.Parse(request.ProductSelectionRule);

            if (request.CategoryIds is not null)
            {
                var categories = await _unitOfWork.Repository<Category>().Query()
                    .Where(c => request.CategoryIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);
                if (categories.Count != request.CategoryIds.Distinct().Count())
                    return Result.Failure<HomeSectionDto>(Error.New("Category.NotFound", "One or more categories were not found."));

                section.SectionCategories = categories
                    .Select(c => new HomeSectionCategory { HomeSectionId = section.Id, CategoryId = c.Id })
                    .ToList();
            }

            if (request.OccasionIds is not null)
            {
                var occasions = await _unitOfWork.Repository<Occasion>().Query()
                    .Where(o => request.OccasionIds.Contains(o.Id))
                    .ToListAsync(cancellationToken);
                if (occasions.Count != request.OccasionIds.Distinct().Count())
                    return Result.Failure<HomeSectionDto>(Error.New("Occasion.NotFound", "One or more occasions were not found."));

                section.SectionOccasions = occasions
                    .Select(o => new HomeSectionOccasion { HomeSectionId = section.Id, OccasionId = o.Id })
                    .ToList();
            }

            if (request.ProductIds is not null)
            {
                var products = await _unitOfWork.Repository<Product>().Query()
                    .Where(p => request.ProductIds.Contains(p.Id))
                    .ToListAsync(cancellationToken);
                if (products.Count != request.ProductIds.Distinct().Count())
                    return Result.Failure<HomeSectionDto>(Error.New("Product.NotFound", "One or more products were not found."));

                section.SectionProducts = request.ProductIds.Select((id, index) =>
                    new HomeSectionProduct { HomeSectionId = section.Id, ProductId = id, DisplayOrder = index }).ToList();
            }

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            section.UpdatedAt = DateTime.UtcNow;
            section.LastChangedBy = currentUserId;

            await _changeLogger.LogAsync("HomeSection", section.Id, "Updated", currentUserId, $"Updated section '{section.Title}'.", cancellationToken);
            repo.Update(section);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await repo.Query()
                .Where(s => s.Id == section.Id)
                .Include(s => s.SectionCategories!).ThenInclude(sc => sc.Category)
                .Include(s => s.SectionOccasions!).ThenInclude(so => so.Occasion)
                .Include(s => s.SectionProducts!).ThenInclude(sp => sp.Product).ThenInclude(p => p.ProductImages)
                .FirstAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new HomeSectionsChangedEvent(currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(HomeSectionMapper.ToDto(updated));
        }
    }
}
