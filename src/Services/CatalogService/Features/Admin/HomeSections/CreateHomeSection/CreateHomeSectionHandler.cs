using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Features.Admin.HomeSections.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.HomeSections.CreateHomeSection
{
    public class CreateHomeSectionHandler : IRequestHandler<CreateHomeSectionCommand, Result<HomeSectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public CreateHomeSectionHandler(
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

        public async Task<Result<HomeSectionDto>> Handle(CreateHomeSectionCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            var section = new HomeSection
            {
                Id = Guid.NewGuid(),
                Type = Enum.Parse<HomeSectionKind>(request.Type, ignoreCase: true),
                Title = request.Title,
                Order = request.Order,
                Enabled = request.Enabled,
                ViewAllDeeplink = request.ViewAllDeeplink,
                BannerImageUrl = request.BannerImageUrl,
                BannerDeeplink = request.BannerDeeplink,
                ProductSelectionRule = string.IsNullOrWhiteSpace(request.ProductSelectionRule)
                    ? null
                    : ProductSelectionRuleParser.Parse(request.ProductSelectionRule),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedBy = currentUserId
            };

            if (request.CategoryIds is { Count: > 0 })
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

            if (request.OccasionIds is { Count: > 0 })
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

            if (request.ProductIds is { Count: > 0 })
            {
                var products = await _unitOfWork.Repository<Product>().Query()
                    .Where(p => request.ProductIds.Contains(p.Id))
                    .ToListAsync(cancellationToken);
                if (products.Count != request.ProductIds.Distinct().Count())
                    return Result.Failure<HomeSectionDto>(Error.New("Product.NotFound", "One or more products were not found."));

                section.SectionProducts = request.ProductIds.Select((id, index) =>
                    new HomeSectionProduct { HomeSectionId = section.Id, ProductId = id, DisplayOrder = index }).ToList();
            }

            await _changeLogger.LogAsync("HomeSection", section.Id, "Created", currentUserId, $"Created {section.Type} section '{section.Title}'.", cancellationToken);
            await _unitOfWork.Repository<HomeSection>().AddAsync(section, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await _unitOfWork.Repository<HomeSection>().Query()
                .Where(s => s.Id == section.Id)
                .Include(s => s.SectionCategories!).ThenInclude(sc => sc.Category)
                .Include(s => s.SectionOccasions!).ThenInclude(so => so.Occasion)
                .Include(s => s.SectionProducts!).ThenInclude(sp => sp.Product).ThenInclude(p => p.ProductImages)
                .FirstAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new HomeSectionsChangedEvent(currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(HomeSectionMapper.ToDto(created));
        }
    }
}
