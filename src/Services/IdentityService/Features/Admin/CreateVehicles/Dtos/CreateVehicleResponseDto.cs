namespace IdentityService.Features.Admin.CreateVehicles.Dtos
{
    public class CreateVehicleResponseDto
    {
        public Guid  Id { get; set; }
        public Guid?  CreatedBy { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

    }
}
