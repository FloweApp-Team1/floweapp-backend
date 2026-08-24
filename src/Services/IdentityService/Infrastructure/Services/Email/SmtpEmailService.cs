namespace IdentityService.Infrastructure.Services.Email
{
    using global::Shared.Contracts;
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

        public Task SendPasswordResetOtpAsync(string email, string otp)
        {
            var message = BuildMessage(email, "Flowers App Password Reset", new TextPart("plain")
            {
                Text =
                    $"""
                    Your password reset verification code is:

                    {otp}

                    This code expires in 10 minutes.

                    If you didn't request a password reset, you can safely ignore this email.
                    """
            });

            // Fire-and-forget from the caller's perspective (see ForgetPasswordHandler) ->
            // never tie this to the inbound request's CancellationToken.
            return SendAsync(message, CancellationToken.None);
        }

        public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default, string? messageId = null)
        {
            var message = BuildMessage(to, subject, new TextPart("html") { Text = htmlBody });
            if (!string.IsNullOrWhiteSpace(messageId))
            {
                message.MessageId = messageId.Contains("@") ? messageId : $"{messageId}@{_settings.SenderEmail.Split('@').LastOrDefault() ?? "flowersapp.com"}";
            }

            return SendAsync(message, cancellationToken);
        }

        private MimeMessage BuildMessage(string to, string subject, MimeEntity body)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = body;

            return message;
        }

        private async Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
        {
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);

            await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            await smtp.SendAsync(message, cancellationToken);

            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }
}
