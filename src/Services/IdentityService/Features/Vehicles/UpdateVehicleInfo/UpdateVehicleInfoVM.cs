namespace IdentityService.Features.Vehicles.UpdateVehicleInfo;

public class UpdateVehicleInfoVM
{
    public Guid VehicleTypeId { get; set; }
    public string PlateNumber { get; set; } = default!;
    public IFormFile? LicenseDocument { get; set; }
}
