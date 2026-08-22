using System.Globalization;
using System.Net;
using System.Text.Json;
using AddressCartService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Shared.Results;

namespace AddressCartService.Infrastructure.Services.Geocoding
{
    // Talks to the Google Maps Geocoding API. Registered as a typed HttpClient so
    // BaseAddress/timeout live in one place (see DependencyInjection.AddInfrastructureServices).
    public class GoogleGeocodingProvider : IGeocodingProvider
    {
        private readonly HttpClient _httpClient;
        private readonly GeocodingSettings _settings;
        private readonly ILogger<GoogleGeocodingProvider> _logger;

        private static readonly string[] CityTypes = ["locality", "postal_town", "administrative_area_level_2"];
        private static readonly string[] AreaTypes = ["sublocality", "sublocality_level_1", "neighborhood", "administrative_area_level_1"];

        public GoogleGeocodingProvider(
            HttpClient httpClient,
            IOptions<GeocodingSettings> settings,
            ILogger<GoogleGeocodingProvider> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<Result<GeocodeAddress>> ReverseGeocodeAsync(
            double lat, double lng, CancellationToken cancellationToken)
        {
            var latlng = $"{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}";
            var requestUri = $"{_settings.BaseUrl}?latlng={Uri.EscapeDataString(latlng)}&key={Uri.EscapeDataString(_settings.ApiKey)}";

            GoogleGeocodingResponse? payload;

            try
            {
                using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    return Result<GeocodeAddress>.Failure(Error.New(
                        "Geocoding.TooManyAttempts", "Geocoding provider rate limit exceeded. Please try again shortly."));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Geocoding provider returned {StatusCode} for reverse geocode lookup", response.StatusCode);

                    return Result<GeocodeAddress>.Failure(Error.New(
                        "Geocoding.ProviderError", "Geocoding provider is temporarily unavailable."));
                }

                payload = await response.Content.ReadFromJsonAsync<GoogleGeocodingResponse>(cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "Reverse geocoding call failed for {Lat},{Lng}", lat, lng);

                return Result<GeocodeAddress>.Failure(Error.New(
                    "Geocoding.ProviderError", "Geocoding provider is temporarily unavailable."));
            }

            if (payload is null)
                return Result<GeocodeAddress>.Failure(Error.New(
                    "Geocoding.ProviderError", "Geocoding provider returned an unexpected response."));

            switch (payload.Status)
            {
                case "OK":
                    break;

                case "ZERO_RESULTS":
                    return Result<GeocodeAddress>.Failure(Error.New(
                        "Geocoding.NotFound", "No address could be found for this location."));

                case "OVER_QUERY_LIMIT":
                    return Result<GeocodeAddress>.Failure(Error.New(
                        "Geocoding.TooManyAttempts", "Geocoding provider rate limit exceeded. Please try again shortly."));

                case "REQUEST_DENIED":
                    _logger.LogError(
                        "Geocoding provider denied the request: {ErrorMessage}", payload.ErrorMessage);
                    return Result<GeocodeAddress>.Failure(Error.New(
                        "Geocoding.ProviderError", "Geocoding provider is temporarily unavailable."));

                case "INVALID_REQUEST":
                    return Result<GeocodeAddress>.Failure(Error.New(
                        "Geocoding.InvalidRequest", "The supplied coordinates are invalid."));

                default:
                    _logger.LogError(
                        "Geocoding provider returned status {Status}: {ErrorMessage}", payload.Status, payload.ErrorMessage);
                    return Result<GeocodeAddress>.Failure(Error.New(
                        "Geocoding.ProviderError", "Geocoding provider is temporarily unavailable."));
            }

            var result = payload.Results.FirstOrDefault();
            if (result is null)
                return Result<GeocodeAddress>.Failure(Error.New(
                    "Geocoding.NotFound", "No address could be found for this location."));

            var city = FirstComponent(result.AddressComponents, CityTypes);
            var area = FirstComponent(result.AddressComponents, AreaTypes);

            return Result<GeocodeAddress>.Success(new GeocodeAddress(
                AddressLine: result.FormattedAddress,
                City: city ?? string.Empty,
                Area: area ?? string.Empty));
        }

        private static string? FirstComponent(List<GoogleAddressComponent> components, string[] preferredTypes) =>
            preferredTypes
                .Select(type => components.FirstOrDefault(c => c.Types.Contains(type))?.LongName)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
    }
}
