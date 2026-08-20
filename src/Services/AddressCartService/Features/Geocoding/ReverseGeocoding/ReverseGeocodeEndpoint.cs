using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace AddressCartService.Features.Geocoding.ReverseGeocoding;

public class ReverseGeocodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/geocoding/reverse", async (
                double lat,
                double lng,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new ReverseGeocodingQuery(lat, lng), cancellationToken);

                return result.ToMinimalApiResult("Coordinates resolved");
            })
            .WithTags("Geocoding")
            .WithName("ReverseGeocode").AllowAnonymous();
            //.RequireAuthorization(AppPolicies.CustomerOnly);
    }
}
