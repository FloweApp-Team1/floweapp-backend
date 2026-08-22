using IdentityService.Domain.Enums;

namespace IdentityService.Features.Admin.DriverApplications.ApproveReviewDriverApplication.Dtos_VM
{
    public class ApproveDriverApplicationDto
    {
        
        public Guid ApplicationId { get; set; } 

        public Guid DeliveryId { get; set; } 
        public DeliveryStatusEnum Status { get; set; }


        public string ReviewedBy { get; set; } = null!;

        public DateTime ReviewedAt { get; set; }
    }

     
  
}
