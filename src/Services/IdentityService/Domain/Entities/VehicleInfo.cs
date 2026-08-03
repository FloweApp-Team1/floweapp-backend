using IdentityService.Domain.Enums;

namespace IdentityService.Domain.Entities
{
    public class VehicleInfo:BaseEntity
    {
        public VehicleTypeEnum Type { get; set; }
        public string PlateNumber { get; set; } = null!;
        public string Capacity { get; set; } = null!; 

        public Delivery Delivery { get; set; }
        public Guid DeliveryId { get; set; }


    }
}
