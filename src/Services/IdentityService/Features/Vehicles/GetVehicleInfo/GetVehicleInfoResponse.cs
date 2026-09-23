namespace IdentityService.Features.Vehicles.GetVehicleInfo;

public sealed record GetVehicleInfoResponse(
    Guid VehicleId,
    Guid VehicleTypeId,
    string VehicleTypeName,
    string PlateNumber,
    int Capacity,
    string LicenseDocument
);
