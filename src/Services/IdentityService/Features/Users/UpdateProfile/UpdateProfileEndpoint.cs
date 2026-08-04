using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/me", () =>
                ApiResponse.Success(new { }, "Profile updated").ToHttpResult())
            .WithTags("Users")
            .WithName("UpdateProfile")
            .RequireAuthorization();
    }
}
