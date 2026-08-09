using IdentityService.Domain.Enums;

namespace IdentityService.Features.Admin.DriverApplications.ListDriverApplications.Dtos
{
    public class DriverApplicationResponseDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public DateTime SubmittedAt { get; set; } 
        public DeliveryStatusEnum Status { get; set; } 
        
    }
}
