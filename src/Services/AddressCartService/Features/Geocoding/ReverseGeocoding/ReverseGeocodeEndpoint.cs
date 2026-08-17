using Shared.Contracts;
using Shared.Responses;

namespace AddressCartService.Features.Geocoding.ReverseGeocoding;

public class ReverseGeocodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/geocoding/reverse", () =>
                ApiResponse.Success(new { }, "Coordinates resolved").ToHttpResult())
            .WithTags("Geocoding")
            .WithName("ReverseGeocode")
            .AllowAnonymous();
    }
}
