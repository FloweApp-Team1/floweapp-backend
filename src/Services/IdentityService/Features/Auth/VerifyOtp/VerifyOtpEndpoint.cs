using IdentityService.Common.Contracts;

namespace IdentityService.Features.Auth.VerifyOtp;

public class VerifyOtpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/otp-verification", () => { })
            .WithTags("Auth")
            .WithName("VerifyOtp")
            .AllowAnonymous();
    }
}