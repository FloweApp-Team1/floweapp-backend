using IdentityService.Common.Contracts;
using MediatR;

namespace IdentityService.Features.Auth.ForgetPassword;

public class ForgetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forget-password",
                async (string email, ISender sender) =>
                {
                    var result = await sender.Send(new ForgetPasswordCommand(email));
                    return result.ToHttpResult();
                })
            .WithTags("Auth")
            .WithName("ForgetPassword")
            .AllowAnonymous();
    }
}
