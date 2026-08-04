namespace IdentityService.Domain.Entities
{
    public class AdminLoginAudit : BaseEntity
    {
        public string Email { get; set; } = null!;
        public DateTime AttemptedAt { get; set; }
        public bool IsSuccess { get; set; }
        public string IpAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;
    }
}
