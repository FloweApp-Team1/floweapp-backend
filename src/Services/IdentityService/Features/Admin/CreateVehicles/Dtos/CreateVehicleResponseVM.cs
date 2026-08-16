namespace IdentityService.Features.Admin.CreateVehicles.Dtos
{
    public class CreateVehicleResponseVM
    {
        public string Id { get; set; } = null!;
        public string? CreatedBy { get; set; } 
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
