using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Auth.RefreshTokens;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh-token", () =>
                ApiResponse.Success(new { }, "Token refreshed").ToHttpResult())
            .WithTags("Auth")
            .WithName("RefreshToken")
            .AllowAnonymous();
    }
}
