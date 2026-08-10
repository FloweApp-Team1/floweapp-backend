using Shared.Contracts;
using Shared.Extensions;
using Shared.Interfaces;
using Shared.Responses;
using MediatR;

namespace IdentityService.Features.Users.ChangePassword
{
    public sealed class ChangePasswordEndpoint : IEndpoint
    {
        public sealed record ChangePasswordRequest(
            string CurrentPassword,
            string NewPassword,
            string ConfirmNewPassword);

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/users/change-password", async (
                ChangePasswordRequest request,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is null)
                    return ApiResponse.Fail(
                        "Authentication required", StatusCodes.Status401Unauthorized).ToHttpResult();

                var command = new ChangePasswordCommand(
                    currentUser.UserId.Value,
                    request.CurrentPassword,
                    request.NewPassword,
                    request.ConfirmNewPassword);

                var result = await sender.Send(command, cancellationToken);

                return result.ToMinimalApiResult(
                    "Password changed successfully. Please log in again.");
            })
            .WithTags("Users")
            .WithName("ChangePassword")
            .RequireAuthorization()
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
        }
    }
}
