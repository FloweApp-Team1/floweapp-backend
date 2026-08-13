using Shared.Contracts;
using Shared.Responses;

namespace IdentityService.Features.Users.UpdateFcmToken;

public class UpdateFcmTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/fcm-token", () =>
                ApiResponse.Success(new { }, "FCM token updated").ToHttpResult())
            .WithTags("Users")
            .WithName("UpdateFcmToken")
            .RequireAuthorization();
    }
}
