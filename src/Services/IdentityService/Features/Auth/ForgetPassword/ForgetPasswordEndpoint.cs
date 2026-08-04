using IdentityService.Common.Contracts;

namespace IdentityService.Features.Auth.ForgetPassword;

public class ForgetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forget-password", () => { })
            .WithTags("Auth")
            .WithName("ForgetPassword")
            .AllowAnonymous();
    }
}
