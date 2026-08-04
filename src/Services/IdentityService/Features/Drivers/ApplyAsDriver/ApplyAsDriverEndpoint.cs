using IdentityService.Common.Contracts;

namespace IdentityService.Features.Drivers.ApplyAsDriver;

public class ApplyAsDriverEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/drivers/apply", () => { })
            .WithTags("Drivers")
            .WithName("ApplyAsDriver")
            .DisableAntiforgery() // multipart/form-data endpoint
            .AllowAnonymous();
    }
}
