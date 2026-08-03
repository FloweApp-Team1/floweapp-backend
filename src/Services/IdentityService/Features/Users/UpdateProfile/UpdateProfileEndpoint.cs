using IdentityService.Common.Contracts;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/me", () => { })
            .WithTags("Users")
            .WithName("UpdateProfile")
            .RequireAuthorization();
    }
}
