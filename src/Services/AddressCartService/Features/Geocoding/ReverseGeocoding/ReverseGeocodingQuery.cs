using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Geocoding.ReverseGeocoding
{
    public sealed record ReverseGeocodingQuery(double Lat, double Lng)
        : IRequest<Result<ReverseGeocodingResponse>>;
}
