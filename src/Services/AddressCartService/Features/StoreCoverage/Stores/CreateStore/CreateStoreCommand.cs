using AddressCartService.Features.StoreCoverage.Common.Dtos;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.Stores.CreateStore
{
    public record CreateStoreCommand(CreateStoreRequest Request) : IRequest<Result<StoreResponse>>;
}
