using AddressCartService.Features.StoreCoverage.Common.Dtos;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.UpdateStore
{
    public record UpdateStoreCommand(Guid StoreId, UpdateStoreRequest Request) : IRequest<Result<StoreResponse>>;
}
