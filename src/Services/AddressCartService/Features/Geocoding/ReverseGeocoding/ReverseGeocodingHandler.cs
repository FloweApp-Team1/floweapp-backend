using AddressCartService.Infrastructure.Services.Geocoding;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Geocoding.ReverseGeocoding
{
    public sealed class ReverseGeocodingHandler(IGeocodingProvider geocodingProvider)
        : IRequestHandler<ReverseGeocodingQuery, Result<ReverseGeocodingResponse>>
    {
        public async Task<Result<ReverseGeocodingResponse>> Handle(
            ReverseGeocodingQuery request, CancellationToken cancellationToken)
        {
            var geocodeResult = await geocodingProvider.ReverseGeocodeAsync(
                request.Lat, request.Lng, cancellationToken);

            if (geocodeResult.IsFailure)
                return Result<ReverseGeocodingResponse>.Failure(geocodeResult.Error);

            var address = geocodeResult.Value;

            return Result<ReverseGeocodingResponse>.Success(new ReverseGeocodingResponse(
                address.AddressLine,
                address.City,
                address.Area,
                request.Lat,
                request.Lng));
        }
    }
}
