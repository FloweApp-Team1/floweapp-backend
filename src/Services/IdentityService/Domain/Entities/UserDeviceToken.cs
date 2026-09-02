using Shared.Domain;

namespace IdentityService.Domain.Entities
{
    public class UserDeviceToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string DeviceId { get; set; } = null!;
        public string FcmToken { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
