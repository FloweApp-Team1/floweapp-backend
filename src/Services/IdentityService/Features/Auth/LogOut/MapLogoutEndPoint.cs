using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using MediatR;
using Shared.Interfaces;

namespace IdentityService.Features.Auth.LogOut
{
    public sealed class LogoutEndPoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/logout", async (
                LogoutRequestDto request,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken ct) =>
            {
                if (currentUser.UserId is null)
                    return ApiResponse.Fail(
                        "Authentication required", StatusCodes.Status401Unauthorized).ToHttpResult();

                var result = await sender.Send(
                    new LogoutCommand(currentUser.UserId.Value, request.RefreshToken, request.DeviceId), ct);
                return result.ToMinimalApiResult("Logged out successfully.");
            })
            .RequireAuthorization()
            .WithName("Logout")
            .WithTags("Auth")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
        }
    }
}
