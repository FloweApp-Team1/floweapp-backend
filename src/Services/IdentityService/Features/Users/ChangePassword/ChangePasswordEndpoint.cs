using IdentityService.Common.Contracts;

namespace IdentityService.Features.Users.ChangePassword;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/change-password", () => { })
            .WithTags("Users")
            .WithName("ChangePassword")
            .RequireAuthorization();
    }
}