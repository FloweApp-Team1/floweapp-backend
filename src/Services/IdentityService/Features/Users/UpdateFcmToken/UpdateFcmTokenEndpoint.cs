using IdentityService.Common.Contracts;

namespace IdentityService.Features.Users.UpdateFcmToken;

public class UpdateFcmTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/fcm-token", () => { })
            .WithTags("Users")
            .WithName("UpdateFcmToken")
            .RequireAuthorization();
    }
}