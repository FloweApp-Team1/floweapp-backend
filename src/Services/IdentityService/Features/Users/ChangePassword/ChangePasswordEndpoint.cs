using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Users.ChangePassword;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/change-password", () =>
                ApiResponse.Success(new { }, "Password changed").ToHttpResult())
            .WithTags("Users")
            .WithName("ChangePassword")
            .RequireAuthorization();
    }
}
