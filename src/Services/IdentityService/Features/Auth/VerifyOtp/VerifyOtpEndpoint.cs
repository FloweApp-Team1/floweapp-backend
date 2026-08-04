using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;

namespace IdentityService.Features.Auth.VerifyOtp;

public class VerifyOtpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/otp-verification", () =>
                ApiResponse.Success(new { }, "OTP verified").ToHttpResult())
            .WithTags("Auth")
            .WithName("VerifyOtp")
            .AllowAnonymous();
    }
}
