
namespace IdentityService.Features.Admin.AdminLogin
{
    public sealed record LoginAdminResponseDto(
        UserProfileDto User,
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt);
}
