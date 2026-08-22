namespace AddressCartService.Infrastructure.Settings
{
    public class GeocodingSettings
    {
        public string ApiKey { get; set; } = null!;
        public string BaseUrl { get; set; } = "https://maps.googleapis.com/maps/api/geocode/json";
        public int TimeoutSeconds { get; set; } = 10;

        // When true, IGeocodingProvider resolves to MockGeocodingProvider instead of
        // GoogleGeocodingProvider - lets the endpoint be exercised locally without a
        // real API key or burning Google's quota.
        public bool UseMockProvider { get; set; }
    }
}
