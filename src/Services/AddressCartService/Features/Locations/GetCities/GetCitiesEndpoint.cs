using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts;
using Shared.Extensions;

namespace AddressCartService.Features.Locations.GetCities
{
    public class GetCitiesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/locations/governorates/{governorateId}/cities", async (
                    int governorateId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetCitiesQuery(governorateId), cancellationToken);

                    return result.ToMinimalApiResult("Cities retrieved");
                })
                .WithTags("Locations")
                .WithName("GetCities");
        }
    }
}
