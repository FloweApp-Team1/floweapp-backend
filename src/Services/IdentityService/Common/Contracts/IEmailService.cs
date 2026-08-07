namespace IdentityService.Common.Contracts
{
    public interface IEmailService
    {
        Task SendPasswordResetOtpAsync(string email, string otp);

        Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
