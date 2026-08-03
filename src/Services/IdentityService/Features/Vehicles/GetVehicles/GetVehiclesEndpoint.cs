using IdentityService.Common.Contracts;

namespace IdentityService.Features.Vehicles.GetVehicles;

public class GetVehiclesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/vehicles", () => { })
            .WithTags("Vehicles")
            .WithName("GetVehicles")
            .AllowAnonymous();
    }
}
