using Shared.Results;

namespace AddressCartService.Infrastructure.Services.Geocoding
{
    // Stand-in for GoogleGeocodingProvider, enabled via Geocoding__UseMockProvider.
    // Returns canned data instantly - no network call, no API key needed - so the
    // reverse-geocoding flow can be exercised end-to-end locally or in tests.
    public class MockGeocodingProvider : IGeocodingProvider
    {
        private readonly ILogger<MockGeocodingProvider> _logger;

        public MockGeocodingProvider(ILogger<MockGeocodingProvider> logger)
        {
            _logger = logger;
        }

        public Task<Result<GeocodeAddress>> ReverseGeocodeAsync(
            double lat, double lng, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "MockGeocodingProvider resolving {Lat},{Lng} - no real provider is being called", lat, lng);

            // Reserved sentinel coordinate so callers can exercise the "no coverage"
            // failure path (and the frontend's manual-entry fallback) on demand.
            if (lat == 0 && lng == 0)
                return Task.FromResult(Result<GeocodeAddress>.Failure(Error.New(
                    "Geocoding.NotFound", "No address could be found for this location.")));

            var address = new GeocodeAddress(
                AddressLine: $"[MOCK] {Math.Abs(lat):F4} Test Street",
                City: "Cairo",
                Area: "Nasr City");

            return Task.FromResult(Result<GeocodeAddress>.Success(address));
        }
    }
}
