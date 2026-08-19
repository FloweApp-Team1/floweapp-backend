using IdentityService.Infrastructure.Services.OTP.Models;

namespace IdentityService.Common.Contracts
{
    public interface IResetTokenService
    {
        Task<(string Token, int ExpiresInMinutes)> GenerateAsync(string email);

        Task<PasswordResetToken?> ValidateAsync(string token);

        Task InvalidateAsync(string token);
    }

}
