namespace OrdersService.Infrastructure.Services.Email
{
    using global::Shared.Contracts;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public Task SendPasswordResetOtpAsync(string email, string otp)
        {
            return Task.CompletedTask; // Not used in OrdersService
        }

        public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var message = BuildMessage(to, subject, new TextPart("plain") { Text = htmlBody });
            return SendInternalAsync(message, cancellationToken);
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

        private async Task SendInternalAsync(MimeMessage message, CancellationToken cancellationToken)
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }
}
