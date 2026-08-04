using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using MediatR;

namespace IdentityService.Features.Auth.VerifyOtp;

public class VerifyOtpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/otp-verification", 
            async (string email, string otp, ISender sender) =>
            {
                var result = await sender.Send(new VerifyOtpCommand(email, otp));
                return result.ToHttpResult();
            })
            .WithTags("Auth")
            .WithName("VerifyOtp")
            .AllowAnonymous();
    }
}
