namespace IdentityService.Common.Models
{

    public record TokenResponse(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt);
}
