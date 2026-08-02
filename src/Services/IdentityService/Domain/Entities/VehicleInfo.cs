using IdentityService.Domain.Enums;

namespace IdentityService.Domain.Entities
{
    public class VehicleInfo
    {
        public Guid Id { get; set; } 
        public VehicleTypeEnum Type { get; set; }
        public string PlateNumber { get; set; } = null!;
        public string Capacity { get; set; } = null!;


    }
}
