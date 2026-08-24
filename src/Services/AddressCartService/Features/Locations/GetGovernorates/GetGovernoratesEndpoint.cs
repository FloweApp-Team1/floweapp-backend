using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts;
using Shared.Extensions;

namespace AddressCartService.Features.Locations.GetGovernorates
{
    public class GetGovernoratesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/locations/governorates", async (
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetGovernoratesQuery(), cancellationToken);

                    return result.ToMinimalApiResult("Governorates retrieved");
                })
                .WithTags("Locations")
                .WithName("GetGovernorates");
        }
    }
}
