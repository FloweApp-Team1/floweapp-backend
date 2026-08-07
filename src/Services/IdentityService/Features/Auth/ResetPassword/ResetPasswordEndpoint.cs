using IdentityService.Common.Contracts;
using MediatR;

namespace IdentityService.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint : IEndpoint
{
    // Complex type -> bound from the JSON body. The old signature bound these as
    // scalar query-string parameters, which put the new password in the URL
    // (server/proxy access logs, browser history, Referer headers).
    public sealed record Request(string OtpToken, string Password, string ConfirmPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password",
                async (Request request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new ResetPasswordCommand(request.OtpToken, request.Password, request.ConfirmPassword),
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Auth")
            .WithName("ResetPassword")
            .AllowAnonymous();
    }
}
