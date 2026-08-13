using Shared.Models;
using Shared.Results;
using IdentityService.Domain.Enums;
using IdentityService.Features.Drivers.Dtos_VM;
using MediatR;

namespace IdentityService.Features.Drivers.ApplyAsDriver
{
    public record ApplyDriverRequestCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    GenderEnum Gender,  
    string VehiclePlateNumber,
    Guid VehicleTypeId,
    int VehicleCapacity,
    IFormFile LicenceImage,
    string Nid,
    IFormFile NidImage,
    string Password,
    string ConfirmPassword,
    string FcmToken 
    ) : IRequest<Result<ApplyDriverResponseDto>>;
  
}
