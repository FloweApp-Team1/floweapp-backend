using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Addresses.UpdateAddress
{
    public sealed record UpdateAddressCommand(
       Guid AddressId,
       string RecipientName,
       string RecipientPhone,
       string AddressLine,
       string City,
       string Area,
       string? Label,
       double? Lat,
       double? Lng) : IRequest<Result<UpdateAddressResponse>>;
}
