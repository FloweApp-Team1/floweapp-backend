using IdentityService.Common.Interfaces;
using IdentityService.Features.Auth.Sessions.Commends;
using IdentityService.Features.Auth.Sessions.Queries;
using MediatR;

namespace IdentityService.Features.Auth.Sessions
{
    public static class SessionsEndpoints
    {
        public static IEndpointRouteBuilder MapSessionsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/auth/sessions", async (
                string? currentToken,
                ICurrentUserService currentUser,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is null)
                    return Results.Unauthorized();

                var result = await mediator.Send(
                    new GetActiveSessionsQuery(currentUser.UserId.Value, currentToken),
                    cancellationToken);

                return Results.Ok(result.Value);
            })
            .RequireAuthorization()
            .WithTags("Auth")
            .WithName("GetActiveSessions");

            app.MapDelete("/api/auth/sessions/{sessionId:guid}", async (
                Guid sessionId,
                ICurrentUserService currentUser,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is null)
                    return Results.Unauthorized();

                var result = await mediator.Send(
                    new RevokeSessionCommand(currentUser.UserId.Value, sessionId),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.NoContent()
                    : Results.NotFound(new { error = result.Error });
            })
            .RequireAuthorization()
            .WithTags("Auth")
            .WithName("RevokeSession");

            return app;
        }

    }
}
