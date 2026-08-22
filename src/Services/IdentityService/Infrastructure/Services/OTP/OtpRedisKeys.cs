namespace IdentityService.Infrastructure.Services.OTP
{
    public static class OtpRedisKeys
    {
        public static string Otp(string email)
            => $"password-reset:otp:{Normalize(email)}";

        public static string Cooldown(string email)
            => $"password-reset:cooldown:{Normalize(email)}";

        public static string ResetToken(string token)
            => $"password-reset:token:{token}";

        private static string Normalize(string email)
            => email.Trim().ToLowerInvariant();
    }
}
