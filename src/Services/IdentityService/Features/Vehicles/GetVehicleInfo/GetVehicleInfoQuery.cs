using MediatR;
using Shared.Results;

namespace IdentityService.Features.Vehicles.GetVehicleInfo;

public sealed record GetVehicleInfoQuery() : IRequest<Result<GetVehicleInfoResponse>>;
