using IdentityService.Features.Admin.CreateVehicles.Dtos;
using MediatR;
using Shared.Results;

namespace IdentityService.Features.Admin.CreateVehicles
{
    public record CreateVehiclesCommand(string Name):IRequest<Result<CreateVehicleResponseDto>>;
    
}
