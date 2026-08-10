using IdentityService.Domain.Enums;

using Shared.Domain;
namespace IdentityService.Domain.Entities
{
    public class VehicleInfo:BaseEntity
    {
        public VehicleTypeEnum Type { get; set; }
        public string PlateNumber { get; set; } = null!;
        public int Capacity { get; set; } 

        public Delivery Delivery { get; set; }
        public Guid DeliveryId { get; set; }


    }
}
