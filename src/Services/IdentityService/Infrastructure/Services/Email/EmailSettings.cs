namespace IdentityService.Infrastructure.Services.Email
{
    public sealed class EmailSettings
    {
        public string SmtpHost { get; init; } = default!;

        public int SmtpPort { get; init; }

        public string SenderEmail { get; init; } = default!;

        public string SenderName { get; init; } = default!;

        public string Username { get; init; } = default!;

        public string Password { get; init; } = default!;

        public bool EnableSsl { get; init; }
    }
}
