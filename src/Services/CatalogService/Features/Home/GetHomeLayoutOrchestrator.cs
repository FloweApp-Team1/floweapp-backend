using CatalogService.Domain.Entities;
using CatalogService.Features.Home.Dtos;
using CatalogService.Features.Home.Queries;
using MediatR;
using Shared.Contracts;
using Shared.Results;

namespace CatalogService.Features.Home
{
    public sealed class GetHomeLayoutOrchestrator : IRequestHandler<GetHomeLayoutQuery, Result<List<HomeLayoutSectionDto>>>
    {
        private readonly ISender _sender;
        private readonly IRedisCacheService _cacheService;
        private const string CacheKey = "catalog:home:layout";

        public GetHomeLayoutOrchestrator(ISender sender, IRedisCacheService cacheService)
        {
            _sender = sender;
            _cacheService = cacheService;
        }

        public async Task<Result<List<HomeLayoutSectionDto>>> Handle(GetHomeLayoutQuery request, CancellationToken cancellationToken)
        {
            var cached = await _cacheService.GetAsync<List<HomeLayoutSectionDto>>(CacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var sectionsResult = await _sender.Send(new GetHomeLayoutSectionsQuery(), cancellationToken);
            if (sectionsResult.IsFailure)
            {
                return Result.Failure<List<HomeLayoutSectionDto>>(sectionsResult.Error);
            }

            var dtos = new List<HomeLayoutSectionDto>();

            foreach (var section in sectionsResult.Value)
            {
                BaseSectionPayloadDto payloadDto = section.Payload switch
                {
                    BannerPayload b => new BannerPayloadDto
                    {
                        ImageUrl = b.ImageUrl,
                        ClickAction = b.ClickAction
                    },
                    CategoryRailPayload c => new CategoryRailPayloadDto
                    {
                        Items = (await _sender.Send(new GetTopCategoriesQuery(c.Count), cancellationToken)).Value
                    },
                    ProductRailPayload bs => new ProductRailPayloadDto
                    {
                        Items = (await _sender.Send(new GetBestSellersQuery(bs.Count), cancellationToken)).Value
                    },
                    OccasionRailPayload oc => new OccasionRailPayloadDto
                    {
                        Items = (await _sender.Send(new GetTopOccasionsQuery(oc.Count), cancellationToken)).Value
                    },
                    _ => throw new InvalidOperationException($"Unknown payload type: {section.Payload.GetType().Name}")
                };

                dtos.Add(new HomeLayoutSectionDto
                {
                    Id = section.Id,
                    Type = section.type.ToString().ToLower(),
                    Title = section.title,
                    Order = section.order,
                    IsEnabled = section.isEnabled,
                    Payload = payloadDto
                });
            }

            await _cacheService.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(10));

            return Result.Success(dtos);
        }
    }
}
