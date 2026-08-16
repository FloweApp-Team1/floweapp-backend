using IdentityService.Domain.Enums;

namespace IdentityService.Features.Admin.DriverApplications.GetDriverApplication.Dtos
{
    public class DriverApplicationDetailsDto
    {
        public Guid Id { get; set; } 

        // Applicant
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public GenderEnum Gender { get; set; } 

        // Vehicle
        public string VehiclePlateNumber { get; set; } = default!;
        public string VehicleType { get; set; } = default!;
        public int VehicleCapacity { get; set; }

        // Documents
        public string LicenceImageUrl { get; set; } = default!;
        public string Nid { get; set; } = default!;
        public string NidImageUrl { get; set; } = default!;

        // Application
        public DeliveryStatusEnum  Status  { get; set; } 
        public string? RejectReason { get; set; }

        // Review
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
}
