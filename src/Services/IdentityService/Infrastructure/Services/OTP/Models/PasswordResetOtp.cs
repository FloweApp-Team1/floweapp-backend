namespace IdentityService.Infrastructure.Services.OTP.Models
{
    public sealed class PasswordResetOtp
    {
        public string OtpHash { get; init; } = default!;

        public int RemainingAttempts { get; set; } = 5;

        public DateTime ExpiresAtUtc { get; init; }

        public DateTime CreatedAtUtc { get; init; }
    }
}
