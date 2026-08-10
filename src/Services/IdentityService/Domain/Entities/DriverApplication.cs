using IdentityService.Domain.Enums;

using Shared.Domain;
namespace IdentityService.Domain.Entities
{
    public class DriverApplication : BaseEntity
    {
        // Personal Information
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public GenderEnum Gender { get; set; }

        // Authentication
        public string PasswordHash { get; set; } = default!;

        // Vehicle Information
        public string VehiclePlateNumber { get; set; } = default!;
        public VehicleTypeEnum VehicleType { get; set; }
        public int VehicleCapacity { get; set; }

        // Documents
        public string LicenceImageUrl { get; set; } = default!;
        public string NationalId { get; set; } = default!;
        public string NationalIdImageUrl { get; set; } = default!;

        // Application Status
        public DeliveryStatusEnum Status { get; set; }
            = DeliveryStatusEnum.Pending;

        public NotificationStatusEnum NotificationStatus { get; set; } = NotificationStatusEnum.on;

        // Review
        public string? RejectReason { get; set; }

        public Guid? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        // Audit
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
