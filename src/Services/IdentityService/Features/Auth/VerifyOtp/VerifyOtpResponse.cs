namespace IdentityService.Features.Auth.VerifyOtp
{
    public sealed record VerifyOtpResponse(
     string OtpToken,
     int ExpiresInMinutes);
}
