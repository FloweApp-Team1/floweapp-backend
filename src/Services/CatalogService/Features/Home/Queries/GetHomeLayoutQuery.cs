using CatalogService.Features.Home.Dtos;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Home.Queries
{
    public class GetHomeLayoutQuery : IRequest<Result<List<HomeLayoutSectionDto>>>
    {
        public Guid? StoreId { get; }

        public GetHomeLayoutQuery(Guid? storeId = null)
        {
            StoreId = storeId;
        }
    }
}
