using IdentityService.Common.Contracts;

namespace IdentityService.Features.Users.GetProfile;

public class GetProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", () => { })
            .WithTags("Users")
            .WithName("GetProfile")
            .RequireAuthorization();
    }
}