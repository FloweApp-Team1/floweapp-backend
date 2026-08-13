using Shared.Contracts;
using Shared.Responses;

namespace IdentityService.Features.Users.GetProfile;

public class GetProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", () =>
                ApiResponse.Success(new { }, "Profile retrieved").ToHttpResult())
            .WithTags("Users")
            .WithName("GetProfile")
            .RequireAuthorization();
    }
}
