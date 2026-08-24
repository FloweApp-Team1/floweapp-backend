using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Addresses.CreateAddress
{
    public record CreateAddressCommand(
        string RecipientName,
        string RecipientPhone,
        string AddressLine,
        int GovernorateId,
        int CityId,
        string Area,
        double? Lat,
        double? Lng,
        string? Label) : IRequest<Result<CreateAddressResponse>>;
}
