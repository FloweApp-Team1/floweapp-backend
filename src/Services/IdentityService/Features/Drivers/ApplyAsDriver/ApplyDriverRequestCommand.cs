using IdentityService.Common.Models;
using IdentityService.Domain.Enums;
using IdentityService.Features.Drivers.Dtos_VM;
using MediatR;

namespace IdentityService.Features.Drivers.ApplyAsDriver
{
    public record ApplyDriverRequestCommand(
    string Name,
    string Email,
    string Phone,
    GenderEnum Gender,
    DateOnly BirthDate,
    string VehiclePlateNumber,
    VehicleTypeEnum VehicleType,
    int VehicleCapacity,
    IFormFile LicenceImage,
    string Nid,
    IFormFile NidImage,
    string Password,
    string ConfirmPassword,
    string? FcmToken) : IRequest<Result<ApplyDriverDto>>;
  
}
