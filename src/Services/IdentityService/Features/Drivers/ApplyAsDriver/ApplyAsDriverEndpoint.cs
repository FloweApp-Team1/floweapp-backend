using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Drivers.ApplyAsDriver;

public class ApplyAsDriverEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/drivers/apply", () =>
                ApiResponse.Success(new { }, "Driver application submitted", StatusCodes.Status201Created).ToHttpResult())
            .WithTags("Drivers")
            .WithName("ApplyAsDriver")
            .DisableAntiforgery() // multipart/form-data endpoint
            .AllowAnonymous();
    }
}
