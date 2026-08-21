using Shared.Results;

namespace AddressCartService.Infrastructure.Services.Geocoding
{
    public interface IGeocodingProvider
    {
        Task<Result<GeocodeAddress>> ReverseGeocodeAsync(double lat, double lng, CancellationToken cancellationToken);
    }
}
