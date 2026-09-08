using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace IdentityService.Features.Vehicles.UpdateVehicleInfo;

public class UpdateVehicleInfoCommandHandler(
    ICurrentUserService _currentUser,
    IUnitOfWork _unitOfWork,
    IImageService _imageService)
    : IRequestHandler<UpdateVehicleInfoCommand, Result<UpdateVehicleInfoResponse>>
{
    public async Task<Result<UpdateVehicleInfoResponse>> Handle(
        UpdateVehicleInfoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<UpdateVehicleInfoResponse>
                .Failure(Error.New("Vehicles.Unauthorized", "Unauthorized"));
        }

        // Verify the VehicleType exists
        var vehicleTypeExists = await _unitOfWork.Repository<VehicleType>()
            .ExistsAsync(vt => vt.Id == request.VehicleTypeId && !vt.IsDeleted);

        if (!vehicleTypeExists)
        {
            return Result<UpdateVehicleInfoResponse>
                .Failure(Error.New("Vehicles.VehicleType.NotFound", "Vehicle type not found."));
        }

        // Load the VehicleInfo that belongs to the current delivery user
        var vehicleInfo = await _unitOfWork.Repository<VehicleInfo>()
            .Query()
            .Include(v => v.Delivery)
            .FirstOrDefaultAsync(v => v.DeliveryId == userId.Value, cancellationToken);

        if (vehicleInfo is null)
        {
            return Result<UpdateVehicleInfoResponse>
                .Failure(Error.New("Vehicles.NotFound", "Vehicle info not found."));
        }

        // Handle optional license document upload
        string? newLicenseDocumentPath = null;
        if (request.LicenseDocument is not null)
        {
            newLicenseDocumentPath = await _imageService.UploadImageAsync(
                request.LicenseDocument,
                "Drivers",
                userId.Value.ToString(),
                cancellationToken);
        }

        try
        {
            var oldLicenseDocumentPath = vehicleInfo.Delivery.LicenseDocument;

            // Update VehicleInfo fields
            vehicleInfo.VehicleTypeId = request.VehicleTypeId;
            vehicleInfo.PlateNumber = request.PlateNumber;

            // Update LicenseDocument on the linked Delivery only if a new file was uploaded
            if (newLicenseDocumentPath is not null)
            {
                vehicleInfo.Delivery.LicenseDocument = newLicenseDocumentPath;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Delete the old license document file after a successful save
            if (newLicenseDocumentPath is not null && !string.IsNullOrEmpty(oldLicenseDocumentPath))
            {
                await _imageService.DeleteImageAsync(oldLicenseDocumentPath, cancellationToken);
            }

            return Result<UpdateVehicleInfoResponse>
                .Success(new UpdateVehicleInfoResponse(
                    vehicleInfo.Id,
                    vehicleInfo.VehicleTypeId,
                    vehicleInfo.PlateNumber,
                    vehicleInfo.Delivery.LicenseDocument));
        }
        catch (Exception)
        {
            // Roll back the newly uploaded file if the DB save failed
            if (newLicenseDocumentPath is not null)
            {
                await _imageService.DeleteImageAsync(newLicenseDocumentPath, cancellationToken);
            }

            return Result<UpdateVehicleInfoResponse>
                .Failure(Error.New("Vehicles.UpdateFailed", "Failed to update vehicle info. Please try again."));
        }
    }
}
