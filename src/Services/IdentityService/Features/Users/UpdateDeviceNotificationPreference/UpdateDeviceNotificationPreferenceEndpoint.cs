using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;

namespace IdentityService.Features.Users.UpdateDeviceNotificationPreference;

public sealed class UpdateDeviceNotificationPreferenceEndpoint : IEndpoint
{
    public sealed record Request(bool? Enabled);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/devices/{deviceId}/notifications", async (
            string deviceId,
            [FromBody] Request request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateDeviceNotificationPreferenceCommand(deviceId, request.Enabled),
                cancellationToken);

            return result.ToMinimalApiResult("Notification preference updated");
        })
        .WithTags("Devices")
        .WithName("UpdateDeviceNotificationPreference")
        .RequireAuthorization()
        .Produces<ApiResponse<UpdateDeviceNotificationPreferenceResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
