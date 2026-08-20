namespace AddressCartService.Features.Addresses.SetDefaultAddress
{
    public sealed record SetDefaultAddressResponse(
        Guid AddressId,
        bool IsDefault,
        DateTime UpdatedAt);

}
