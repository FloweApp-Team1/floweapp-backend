using AddressCartService.Features.StoreCoverage.Common.Dtos;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.GetStore
{
    public record GetStoreQuery(Guid StoreId) : IRequest<Result<StoreResponse>>;
}
