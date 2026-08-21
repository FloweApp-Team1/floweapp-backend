namespace AddressCartService.Features.Addresses.GetAddress
{
    public sealed record AddressDetailsResponse(
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
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
 
