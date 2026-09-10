namespace IdentityService.Features.Vehicles.UpdateVehicleInfo;

public sealed record UpdateVehicleInfoResponse(
    Guid VehicleId,
    Guid VehicleTypeId,
    string PlateNumber,
    string LicenseDocument
);
