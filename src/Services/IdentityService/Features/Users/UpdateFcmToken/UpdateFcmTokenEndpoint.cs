using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Users.UpdateFcmToken;

public class UpdateFcmTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/devices/fcm-token", async (
            [FromBody] UpdateFcmTokenCommand request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(request, cancellationToken);

            return result.ToMinimalApiResult("FCM token updated");
        })
        .WithTags("Devices")
        .WithName("UpdateFcmToken")
        .RequireAuthorization()
        .Produces<ApiResponse<UpdateFcmTokenResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
