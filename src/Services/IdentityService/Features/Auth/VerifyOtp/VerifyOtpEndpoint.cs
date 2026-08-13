using Shared.Contracts;
using Shared.Responses;
using MediatR;

namespace IdentityService.Features.Auth.VerifyOtp;

public class VerifyOtpEndpoint : IEndpoint
{
    public sealed record Request(string Email, string Otp);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/otp-verification",
            async (Request request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new VerifyOtpCommand(request.Email, request.Otp), cancellationToken);
                return result.ToHttpResult();
            })
            .WithTags("Auth")
            .WithName("VerifyOtp")
            .AllowAnonymous();
    }
}
