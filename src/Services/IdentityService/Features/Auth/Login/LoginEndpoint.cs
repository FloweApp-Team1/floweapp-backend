using IdentityService.Common.Contracts;

namespace IdentityService.Features.Auth.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", () => { })
            .WithTags("Auth")
            .WithName("Login")
            .AllowAnonymous();
    }
}