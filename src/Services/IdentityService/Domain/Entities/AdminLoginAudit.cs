namespace IdentityService.Domain.Entities
{
    public class AdminLoginAudit : BaseEntity
    {
        public string Email { get; set; } = null!;
        public DateTime AttemptedAt { get; set; }
        public bool IsSuccess { get; set; }
        public string IpAddress { get; set; } = null!;
        private string _userAgent = string.Empty;
        public string UserAgent
        {
            get => _userAgent;
            set => _userAgent = string.IsNullOrEmpty(value)
                                ? "unknown"
                                : value.Length > 500 ? value[..500] : value;
        }
    }
}
