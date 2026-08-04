using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

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
