namespace IdentityService.Common.Settings
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool EnableSsl { get; set; } = true;
    }
}
