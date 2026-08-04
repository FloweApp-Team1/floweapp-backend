using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Auth.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", () =>
                ApiResponse.Success(new { }, "Logged in").ToHttpResult())
            .WithTags("Auth")
            .WithName("Login")
            .AllowAnonymous();
    }
}
