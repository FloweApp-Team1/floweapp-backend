using System.Text.Json.Serialization;

namespace AddressCartService.Infrastructure.Services.Geocoding
{
    // Mirrors only the fields we need from the Google Maps Geocoding API response.
    // https://developers.google.com/maps/documentation/geocoding/requests-reverse-geocoding
    public sealed class GoogleGeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<GoogleGeocodingResult> Results { get; set; } = [];

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }

    public sealed class GoogleGeocodingResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;

        [JsonPropertyName("address_components")]
        public List<GoogleAddressComponent> AddressComponents { get; set; } = [];
    }

    public sealed class GoogleAddressComponent
    {
        [JsonPropertyName("long_name")]
        public string LongName { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = [];
    }
}
