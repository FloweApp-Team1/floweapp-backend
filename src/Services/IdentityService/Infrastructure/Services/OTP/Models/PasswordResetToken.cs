namespace IdentityService.Infrastructure.Services.OTP.Models
{
    public sealed class PasswordResetToken
    {
        public Guid UserId { get; init; }

        public string Email { get; init; } = default!;

        public DateTime ExpiresAtUtc { get; init; }
    }
}
