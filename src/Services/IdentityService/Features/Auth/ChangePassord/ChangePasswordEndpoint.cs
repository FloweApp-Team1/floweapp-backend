using IdentityService.Common.Interfaces;
using IdentityService.Features.Auth.ChangePassord.Commends;
using MediatR;

namespace IdentityService.Features.Auth.ChangePassord
{
    public static class ChangePasswordEndpoint
    {
        public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);

        public static IEndpointRouteBuilder MapChangePasswordEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/change-password", async (
                ChangePasswordRequest request,
                ICurrentUserService currentUser,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                
                if (currentUser.UserId is null)
                    return Results.Unauthorized();

                var command = new ChangePasswordCommand(
                    currentUser.UserId.Value,
                    request.CurrentPassword,
                    request.NewPassword,
                    request.ConfirmNewPassword);

                var result = await mediator.Send(command, cancellationToken);

                if (!result.IsSuccess)
                    return Results.BadRequest(new { error = result.Error });

               
                return Results.Ok(new { message = "Password changed successfully. Please log in again." });
            })
            .RequireAuthorization()
            .WithTags("Auth")
            .WithName("ChangePassword");

            return app;
        }
    }
}
