namespace IdentityService.Domain.Entities
{
    public class OtpCode:BaseEntity
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
