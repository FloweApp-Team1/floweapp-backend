using global::Shared.Contracts;
using global::IdentityService.Common.Contracts;
using IdentityService.Infrastructure.Services.OTP.Models;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Services.OTP
{
    public sealed class ResetTokenService : IResetTokenService
    {
        private readonly IRedisCacheService _cache;

        private const int ExpiryMinutes = 30;

        public ResetTokenService(IRedisCacheService cache)
        {
            _cache = cache;
        }

        public async Task<(string Token, int ExpiresInMinutes)> GenerateAsync(string email)
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            var token = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var model = new PasswordResetToken
            {
                Email = email,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(ExpiryMinutes)
            };

            await _cache.SetAsync(
                OtpRedisKeys.ResetToken(token),
                model,
                TimeSpan.FromMinutes(ExpiryMinutes));

            return (token, ExpiryMinutes);
        }

        public Task<PasswordResetToken?> ValidateAsync(string token)
        {
            return _cache.GetAsync<PasswordResetToken>(OtpRedisKeys.ResetToken(token));
        }

        public Task InvalidateAsync(string token)
        {
            return _cache.RemoveAsync(OtpRedisKeys.ResetToken(token));
        }
    }
}
