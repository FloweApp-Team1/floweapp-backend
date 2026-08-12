namespace IdentityService.Features.Vehicles.GetVehicles.Dtos
{
    public class ListVehiclesDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } 

    }
}
