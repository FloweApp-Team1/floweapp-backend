namespace IdentityService.Domain.Entities
{
    public class RefreshToken:BaseEntity
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        // Navigation property
        public Guid UserId { get; set; } 
        public User User { get; set; } = null!;
    }
}
