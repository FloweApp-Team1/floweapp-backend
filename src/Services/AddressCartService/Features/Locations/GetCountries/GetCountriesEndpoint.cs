using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;

namespace AddressCartService.Features.Locations.GetCountries
{
    public class GetCountriesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/locations/countries", async (
                    [FromQuery] string? search,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetCountriesQuery(search), cancellationToken);

                    return result.ToMinimalApiResult("Countries retrieved");
                })
                .WithTags("Locations")
                .WithName("GetCountries")
                .AllowAnonymous()
                .Produces<ApiResponse<IReadOnlyList<CountryResponse>>>(StatusCodes.Status200OK);
        }
    }
}
