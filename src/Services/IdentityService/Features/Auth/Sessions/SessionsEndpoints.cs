using IdentityService.Common.Contracts;
using IdentityService.Common.Extensions;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Common.Results;
using IdentityService.Common.Responses;
using IdentityService.Features.Auth.Sessions.Commends;
using IdentityService.Features.Auth.Sessions.Queries;
using MediatR;

namespace IdentityService.Features.Auth.Sessions
{
    public sealed class SessionsEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/auth/sessions", async (
                string? currentToken,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is null)
                    return ApiResponse.Fail(
                        "Authentication required", StatusCodes.Status401Unauthorized).ToHttpResult();

                var result = await sender.Send(
                    new GetActiveSessionsQuery(currentUser.UserId.Value, currentToken),
                    cancellationToken);

                return result.ToMinimalApiResult("Active sessions retrieved");
            })
            .RequireAuthorization()
            .WithTags("Auth")
            .WithName("GetActiveSessions")
            .Produces<ApiResponse<List<SessionDto>>>(StatusCodes.Status200OK);

            app.MapDelete("/auth/sessions/{sessionId:guid}", async (
                Guid sessionId,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is null)
                    return ApiResponse.Fail(
                        "Authentication required", StatusCodes.Status401Unauthorized).ToHttpResult();

                var result = await sender.Send(
                    new RevokeSessionCommand(currentUser.UserId.Value, sessionId),
                    cancellationToken);

                return result.ToMinimalApiResult("Session revoked");
            })
            .RequireAuthorization()
            .WithTags("Auth")
            .WithName("RevokeSession")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
        }
    }
}
