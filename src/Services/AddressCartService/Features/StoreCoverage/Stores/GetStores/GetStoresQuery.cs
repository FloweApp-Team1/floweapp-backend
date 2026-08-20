using AddressCartService.Features.StoreCoverage.Common.Dtos;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.GetStores
{
    public record ListStoresQueryParams(int Page = 1, int Limit = 20);

    public record GetStoresQuery(int Page, int PageSize) : IRequest<Result<PagedStoresResult>>;
    public record PagedStoresResult(List<StoreResponse> Items, int TotalCount);
}
