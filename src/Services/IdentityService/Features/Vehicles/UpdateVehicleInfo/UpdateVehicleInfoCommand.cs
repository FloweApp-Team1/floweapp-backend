using MediatR;
using Shared.Results;

namespace IdentityService.Features.Vehicles.UpdateVehicleInfo;

public record UpdateVehicleInfoCommand(
    Guid VehicleTypeId,
    string PlateNumber,
    IFormFile? LicenseDocument
) : IRequest<Result<UpdateVehicleInfoResponse>>;
