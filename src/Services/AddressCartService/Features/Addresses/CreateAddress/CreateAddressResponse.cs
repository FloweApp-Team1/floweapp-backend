namespace AddressCartService.Features.Addresses.CreateAddress
{
    public sealed record CreateAddressResponse(
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
        Guid? StoreId,
        bool IsServiceable,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
