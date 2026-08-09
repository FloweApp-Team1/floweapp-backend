namespace IdentityService.Features.Admin.DriverApplications.GetDriverApplication.Dtos
{
    public class DriverApplicationDetailsVM
    {
        public string Id { get; set; } = default!;

        // Applicant
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Gender { get; set; } = default!;

        // Vehicle
        public string VehiclePlateNumber { get; set; } = default!;
        public string VehicleType { get; set; } = default!;
        public int VehicleCapacity { get; set; }

        // Documents
        public string LicenceImageUrl { get; set; } = default!;
        public string Nid { get; set; } = default!;
        public string NidImageUrl { get; set; } = default!;

        // Application
        public string Status { get; set; } = default!;
        public string? RejectReason { get; set; }

        // Review
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
}
