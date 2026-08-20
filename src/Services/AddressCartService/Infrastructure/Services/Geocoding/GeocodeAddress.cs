namespace AddressCartService.Infrastructure.Services.Geocoding
{
    // Provider-agnostic shape a reverse-geocoding lookup resolves to.
    public sealed record GeocodeAddress(string AddressLine, string City, string Area);
}
