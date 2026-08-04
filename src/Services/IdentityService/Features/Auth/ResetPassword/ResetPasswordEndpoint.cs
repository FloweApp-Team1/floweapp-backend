using IdentityService.Common.Contracts;
using MediatR;

namespace IdentityService.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password",
                async (string otpToken, string password, string confirmPassword, ISender sender) =>
                {
                    var result = await sender.Send(new ResetPasswordCommand(otpToken, password, confirmPassword));
                    return result.ToHttpResult();
                })
            .WithTags("Auth")
            .WithName("ResetPassword")
            .AllowAnonymous();
    }
}
