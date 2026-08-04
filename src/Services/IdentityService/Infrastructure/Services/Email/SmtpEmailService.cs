namespace IdentityService.Infrastructure.Services.Email
{
    using global::IdentityService.Common.Contracts;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Options;
    using MimeKit;

    public sealed class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendPasswordResetOtpAsync(string email,string otp)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = "Flowers App Password Reset";

            message.Body = new TextPart("plain")
            {
                Text =
                    $"""
                    Your password reset verification code is:

                    {otp}

                    This code expires in 10 minutes.

                    If you didn't request a password reset, you can safely ignore this email.
                    """
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync( _settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync( _settings.Username, _settings.Password);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }
    }
}
