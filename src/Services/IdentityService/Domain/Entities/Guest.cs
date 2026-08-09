namespace IdentityService.Domain.Entities
{
    public class Guest : BaseEntity
    {
        public string UserName { get; set; } = null!;

        public string? IpAddress { get; set; }

        public string? DeviceInfo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    }
}
