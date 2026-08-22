using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Addresses.GetAddresses
{
    public class GetAddressesQuery : IRequest<Result<List<AddressResponse>>>
    {
    }
}
