namespace IdentityService.Common.Security
{
    public sealed class JwtSettings
    {
        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;

        public int AccessTokenExpiryMinutes { get; set; }
    }
}
