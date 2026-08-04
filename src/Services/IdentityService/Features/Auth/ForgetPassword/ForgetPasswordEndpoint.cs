using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Auth.ForgetPassword;

public class ForgetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forget-password", () =>
                ApiResponse.Success(new { }, "Password reset instructions sent").ToHttpResult())
            .WithTags("Auth")
            .WithName("ForgetPassword")
            .AllowAnonymous();
    }
}
