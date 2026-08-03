using IdentityService.Common.Contracts;

namespace IdentityService.Features.Auth.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh-token", () => { })
            .WithTags("Auth")
            .WithName("RefreshToken")
            .AllowAnonymous();
    }
}
