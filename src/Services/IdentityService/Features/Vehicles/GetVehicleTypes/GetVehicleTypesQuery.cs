using MediatR;
using Shared.Results;

namespace IdentityService.Features.Vehicles.GetVehicleTypes;

public record GetVehicleTypesQuery : IRequest<Result<IReadOnlyList<VehicleTypeResponse>>>;
