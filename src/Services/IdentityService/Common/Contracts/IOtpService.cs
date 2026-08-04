namespace IdentityService.Common.Contracts
{
    public interface IOtpService
    {
        Task<string?> GenerateAsync(Guid userId, string email);

        Task<VerifyOtpResult> VerifyAsync(string email, string otp);
    }
}
