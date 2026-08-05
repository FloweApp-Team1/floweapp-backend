namespace IdentityService.Features.Auth.Login
{
    public record AuthResponse(string AccessToken, string RefreshToken, string Role);
}
