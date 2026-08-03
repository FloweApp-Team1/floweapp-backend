using IdentityService.Common.Contracts;

namespace IdentityService.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password", () => { })
            .WithTags("Auth")
            .WithName("ResetPassword")
            .AllowAnonymous();
    }
}
