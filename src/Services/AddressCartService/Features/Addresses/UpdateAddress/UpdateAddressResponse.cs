namespace AddressCartService.Features.Addresses.UpdateAddress
{
    public sealed record UpdateAddressResponse(
        Guid Id,
        string RecipientName,
        string RecipientPhone,
        string AddressLine,
        int GovernorateId,
        string GovernorateName,
        int CityId,
        string CityName,
        string Area,
        string? Label,
        double? Lat,
        double? Lng,
        bool IsDefault,
        bool IsServiceable,
        Guid? StoreId,
        DateTime UpdatedAt);
}
