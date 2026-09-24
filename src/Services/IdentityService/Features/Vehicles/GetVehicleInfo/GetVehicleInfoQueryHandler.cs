using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace IdentityService.Features.Vehicles.GetVehicleInfo;

public class GetVehicleInfoQueryHandler(
    ICurrentUserService _currentUser,
    IUnitOfWork _unitOfWork)
    : IRequestHandler<GetVehicleInfoQuery, Result<GetVehicleInfoResponse>>
{
    public async Task<Result<GetVehicleInfoResponse>> Handle(
        GetVehicleInfoQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<GetVehicleInfoResponse>
                .Failure(Error.New("Vehicles.Unauthorized", "Unauthorized"));
        }

        // 1. Retrieve the Delivery record belonging to the authenticated user
        var delivery = await _unitOfWork.Repository<Delivery>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == userId.Value, cancellationToken);

        if (delivery is null)
        {
            return Result<GetVehicleInfoResponse>
                .Failure(Error.New("Drivers.NotFound", "Driver not found."));
        }

        if (delivery.Status != DeliveryStatusEnum.Approved)
        {
            return Result<GetVehicleInfoResponse>
                .Failure(Error.New("Drivers.InvalidState", "Driver application is not approved."));
        }

        // 2. Use the actual Delivery.Id to retrieve VehicleInfo
        var vehicleInfo = await _unitOfWork.Repository<VehicleInfo>()
            .Query()
            .AsNoTracking()
            .Include(v => v.VehicleType)
            .FirstOrDefaultAsync(v => v.DeliveryId == delivery.Id, cancellationToken);

        if (vehicleInfo is null)
        {
            return Result<GetVehicleInfoResponse>
                .Failure(Error.New("Vehicles.NotFound", "Vehicle info not found."));
        }

        return Result<GetVehicleInfoResponse>.Success(new GetVehicleInfoResponse(
            vehicleInfo.Id,
            vehicleInfo.VehicleTypeId,
            vehicleInfo.VehicleType?.Name ?? string.Empty,
            vehicleInfo.PlateNumber,
            vehicleInfo.Capacity,
            delivery.LicenseDocument
        ));
    }
}
