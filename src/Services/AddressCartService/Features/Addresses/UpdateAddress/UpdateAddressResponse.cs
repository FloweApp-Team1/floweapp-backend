namespace AddressCartService.Features.Addresses.UpdateAddress
{
    public sealed record UpdateAddressResponse(
        Guid Id,
        string RecipientName,
        string RecipientPhone,
        string AddressLine,
        string City,
        string Area,
        string? Label,
        double? Lat,
        double? Lng,
        bool IsDefault,
        bool IsServiceable,
        Guid? StoreId,
        DateTime UpdatedAt);
}
