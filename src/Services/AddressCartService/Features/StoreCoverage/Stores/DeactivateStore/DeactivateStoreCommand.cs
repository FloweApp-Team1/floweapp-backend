using AddressCartService.Features.StoreCoverage.Common.Dtos;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.DeactivateStore
{
    public record DeactivateStoreCommand(Guid StoreId) : IRequest<Result<StoreResponse>>;
}
