using IdentityService.Features.Auth.Logout.Commends;
using MediatR;

namespace IdentityService.Features.Auth.Logout
{
    public static class LogoutEndpoint
    {
        public record LogoutRequest(string RefreshToken);

        public static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/logout", async (
                LogoutRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
                
                return Results.Ok(new { message = "Logged out." });
            })
            .RequireAuthorization()
            .WithTags("Auth")
            .WithName("Logout");

            return app;
        }
    }
}
