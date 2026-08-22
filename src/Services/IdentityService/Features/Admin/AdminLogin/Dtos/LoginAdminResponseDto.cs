namespace IdentityService.Features.Admin.AdminLogin.Dtos
{
    public sealed record LoginAdminResponseDto(
        UserProfileDto User,
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt); 
}
