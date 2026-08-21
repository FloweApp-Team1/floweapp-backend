namespace AddressCartService.Features.Geocoding.ReverseGeocoding
{
    public sealed record ReverseGeocodingResponse(
        string AddressLine,
        string City,
        string Area,
        double Lat,
        double Lng);
}
