using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password", () =>
                ApiResponse.Success(new { }, "Password has been reset").ToHttpResult())
            .WithTags("Auth")
            .WithName("ResetPassword")
            .AllowAnonymous();
    }
}
