namespace IdentityService.Features.Auth.Login
{
    public record LoginRequest(string Email, string Password, string FcmToken);
}
