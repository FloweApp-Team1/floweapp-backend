namespace AddressCartService.Features.Addresses.GetAddresses
{
    public record AddressResponse(
        Guid Id,
        string RecipientName,
        string RecipientPhone,
        string AddressLine,
        string City,
        string Area,
        string? Label,
        double? lat,
        double? lng,
        bool IsDefault,
        Guid? StoreId,
        bool IsServiceable,
        DateTime CreatedAt,
        DateTime UpdatedAt);
    
}