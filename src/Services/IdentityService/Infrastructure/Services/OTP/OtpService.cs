using IdentityService.Common.Contracts;
using IdentityService.Infrastructure.Services.OTP.Models;

namespace IdentityService.Infrastructure.Services.OTP
{
    public sealed class OtpService : IOtpService
    {
        private readonly IRedisCacheService _cache;
        private readonly IOtpGenerator _generator;
        private readonly IOtpHasher _hasher;
        private readonly IResetTokenService _resetTokenService;

        private const int ExpiryMinutes = 10;
        private const int CooldownSeconds = 30;

        public OtpService(IRedisCacheService cache, IOtpGenerator generator, IOtpHasher hasher, IResetTokenService resetTokenService)
        {
            _cache = cache;
            _generator = generator;
            _hasher = hasher;
            _resetTokenService = resetTokenService;
        }

        public async Task<string?> GenerateAsync( Guid userId, string email)
        {
            var cooldownKey = OtpRedisKeys.Cooldown(email);

            if (await _cache.ExistsAsync(cooldownKey))
                return null;

            var otp = _generator.Generate();

            var model = new PasswordResetOtp
            {
                OtpHash = _hasher.Hash(otp),
                RemainingAttempts = 5,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(ExpiryMinutes)
            };

            await _cache.SetAsync( OtpRedisKeys.Otp(email), model, TimeSpan.FromMinutes(ExpiryMinutes));

            await _cache.SetAsync( cooldownKey, true, TimeSpan.FromSeconds(CooldownSeconds));

            return otp;
        }

        public async Task<VerifyOtpResult> VerifyAsync(string email, string otp)
        {
            var key = OtpRedisKeys.Otp(email);

            var stored = await _cache.GetAsync<PasswordResetOtp>(key);

            if (stored is null)
                return VerifyOtpResult.Expired();

            if (stored.ExpiresAtUtc < DateTime.UtcNow)
            {
                await _cache.RemoveAsync(key);
                return VerifyOtpResult.Expired();
            }

            if (stored.RemainingAttempts <= 0)
            {
                await _cache.RemoveAsync(key);
                return VerifyOtpResult.AttemptsExceeded();
            }

            if (!_hasher.Verify(otp, stored.OtpHash))
            {
                stored.RemainingAttempts--;

                if (stored.RemainingAttempts <= 0)
                {
                    await _cache.RemoveAsync(key);
                    return VerifyOtpResult.AttemptsExceeded();
                }

                await _cache.SetAsync( key, stored, stored.ExpiresAtUtc - DateTime.UtcNow);

                return VerifyOtpResult.Invalid();
            }

            await _cache.RemoveAsync(key);

            var token = await _resetTokenService.GenerateAsync(email);

            return VerifyOtpResult.SuccessResult(token.Token, token.ExpiresInMinutes);
        }
    }
}
