using IdentityService.Common.Contracts;
using IdentityService.Common.Extensions;
using IdentityService.Common.Responses;
using MediatR;

namespace IdentityService.Features.Auth.LogOut
{
    public sealed class LogoutEndPoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/logout", async (
                LogoutRequestDto request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new LogoutCommand(request.RefreshToken), ct);
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
