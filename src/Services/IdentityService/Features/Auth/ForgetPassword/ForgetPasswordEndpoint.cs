using IdentityService.Common.Contracts;
using MediatR;

namespace IdentityService.Features.Auth.ForgetPassword;

public class ForgetPasswordEndpoint : IEndpoint
{
    // Complex type -> bound from the JSON body, not the query string, so the
    // email address never ends up in a URL (server/proxy logs, browser history).
    public sealed record Request(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forget-password",
                async (Request request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ForgetPasswordCommand(request.Email), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Auth")
            .WithName("ForgetPassword")
            .AllowAnonymous();
    }
}
