using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Addresses.SetDefaultAddress
{
    public sealed record SetDefaultAddressCommand(Guid AddressId)
       : IRequest<Result<SetDefaultAddressResponse>>;
}
