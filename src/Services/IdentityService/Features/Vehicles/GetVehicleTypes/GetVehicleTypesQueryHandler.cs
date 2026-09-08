using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace IdentityService.Features.Vehicles.GetVehicleTypes;

public class GetVehicleTypesQueryHandler : IRequestHandler<GetVehicleTypesQuery, Result<IReadOnlyList<VehicleTypeResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVehicleTypesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VehicleTypeResponse>>> Handle(
        GetVehicleTypesQuery request,
        CancellationToken cancellationToken)
    {
        var vehicleTypes = await _unitOfWork.Repository<VehicleType>()
            .Query()
            .AsNoTracking()
            .Where(vt => !vt.IsDeleted)
            .OrderBy(vt => vt.Name)
            .Select(vt => new VehicleTypeResponse(vt.Id, vt.Name))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<VehicleTypeResponse>>.Success(vehicleTypes);
    }
}
