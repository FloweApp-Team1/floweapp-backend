using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Features.Home.Dtos;
using CatalogService.Features.Home.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Home
{
    public sealed class GetHomeLayoutOrchestrator : IRequestHandler<GetHomeLayoutQuery, Result<List<HomeLayoutSectionDto>>>
    {
        private readonly ISender _sender;
        private readonly IRedisCacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private const string CacheKeyPrefix = "catalog:home:layout";

        public GetHomeLayoutOrchestrator(ISender sender, IRedisCacheService cacheService, IUnitOfWork unitOfWork)
        {
            _sender = sender;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<HomeLayoutSectionDto>>> Handle(GetHomeLayoutQuery request, CancellationToken cancellationToken)
        {
            // Availability (via ProductRail) differs per store, so the cache must be keyed
            // per store too - otherwise the first store to populate the cache would have its
            // stock snapshot served to every other store for the cache's lifetime.
            var cacheKey = request.StoreId is { } storeId
                ? $"{CacheKeyPrefix}:{storeId}"
                : CacheKeyPrefix;

            var cached = await _cacheService.GetAsync<List<HomeLayoutSectionDto>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var sections = await _unitOfWork.Repository<HomeLayoutSection>()
                .GetAll(x => x.IsEnabled)
                .AsNoTracking()
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

            var dtos = new List<HomeLayoutSectionDto>();
            foreach (var section in sections)
            {
                var sectionResult = await BuildSectionDtoAsync(section, request.StoreId, cancellationToken);
                if (sectionResult.IsFailure)
                    continue; // Skip failed sections instead of crashing the entire layout

                dtos.Add(sectionResult.Value);
            }

            await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));

            return Result.Success(dtos);
        }

        private async Task<Result<HomeLayoutSectionDto>> BuildSectionDtoAsync(
            HomeLayoutSection section,
            Guid? storeId,
            CancellationToken cancellationToken)
        {
            BaseSectionPayloadDto payloadDto;

            switch (section.Payload)
            {
                case BannerPayload b:
                    payloadDto = new BannerPayloadDto
                    {
                        ImageUrl = b.ImageUrl,
                        ClickAction = b.ClickAction
                    };
                    break;

                case CategoryRailPayload c:
                    var categoriesResult = await _sender.Send(new GetTopCategoriesQuery(c.Count), cancellationToken);
                    if (categoriesResult.IsFailure)
                        return Result.Failure<HomeLayoutSectionDto>(categoriesResult.Error);
                    payloadDto = new CategoryRailPayloadDto
                    {
                        Items = categoriesResult.Value,
                        ViewAllAction = c.ViewAllAction
                    };
                    break;

                case ProductRailPayload p:
                    var productsResult = await _sender.Send(new GetBestSellersQuery(p.Count, storeId), cancellationToken);
                    if (productsResult.IsFailure)
                        return Result.Failure<HomeLayoutSectionDto>(productsResult.Error);
                    payloadDto = new ProductRailPayloadDto 
                    { 
                        Items = productsResult.Value,
                        ViewAllAction = p.ViewAllAction
                    };
                    break;

                case OccasionRailPayload o:
                    var occasionsResult = await _sender.Send(new GetTopOccasionsQuery(o.Count), cancellationToken);
                    if (occasionsResult.IsFailure)
                        return Result.Failure<HomeLayoutSectionDto>(occasionsResult.Error);
                    payloadDto = new OccasionRailPayloadDto 
                    { 
                        Items = occasionsResult.Value,
                        ViewAllAction = o.ViewAllAction
                    };
                    break;

                default:
                    throw new InvalidOperationException($"Unknown payload type: {section.Payload.GetType().Name}");
            }

            return Result.Success(new HomeLayoutSectionDto
            {
                Id = section.Id,
                Type = ToTypeString(section.Type),
                Title = section.Title,
                Order = section.Order,
                IsEnabled = section.IsEnabled,
                Payload = payloadDto
            });
        }

        private static string ToTypeString(HomeSectionType type) => type switch
        {
            HomeSectionType.Banner       => "banner",
            HomeSectionType.ProductRail  => "product_rail",
            HomeSectionType.OccasionRail => "occasion_rail",
            HomeSectionType.CategoryRail => "category_rail",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
