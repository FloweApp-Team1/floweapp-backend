using Shared.Contracts;
using Shared.Extensions;
using Shared.Interfaces;
using Shared.Responses;
using MediatR;

namespace IdentityService.Features.Auth.RefreshTokens
{
    namespace IdentityService.Features.Auth.RefreshTokens
    {
        public sealed class RefreshTokenEndpoint : IEndpoint
        {
            public sealed record RefreshTokenRequest(string RefreshToken);

            // Matches the frontend contract: data = { token, refreshToken } only.
            public sealed record RefreshTokenResponse(
                string Token,
                string RefreshToken);

            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost("/auth/refresh-token", async (
                    RefreshTokenRequest request,
                    ICurrentUserService currentUser,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new RefreshTokenCommand(
                            request.RefreshToken,
                            currentUser.IpAddress,
                            currentUser.DeviceName),
                        cancellationToken);

                    if (result.IsFailure)
                        return ApiResponse.Fail(
                            result.Error.Message, StatusCodes.Status401Unauthorized).ToHttpResult();

                    var tokens = result.Value;
                    return ApiResponse.Success(
                        new RefreshTokenResponse(
                            tokens.AccessToken,
                            tokens.RefreshToken),
                        "Token refreshed").ToHttpResult();
                })
                .WithTags("Auth")
                .WithName("RefreshToken")
                .AllowAnonymous()
                .Produces<ApiResponse<RefreshTokenResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
            }
        }
    }
}
