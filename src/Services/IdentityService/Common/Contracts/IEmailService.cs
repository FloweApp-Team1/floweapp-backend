namespace IdentityService.Common.Contracts
{
    public interface IEmailService
    {
        Task SendPasswordResetOtpAsync(string email, string otp);
    }
}
