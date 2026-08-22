using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Addresses.DeleteAddress
{
    public sealed record DeleteAddressCommand(Guid AddressId) : IRequest<Result>;
}
