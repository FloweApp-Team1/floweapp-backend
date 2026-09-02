using Shared.Responses;
using IdentityService.Domain.Enums;
using MediatR;

namespace IdentityService.Features.Auth.Register
{
    public record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    GenderEnum Gender,
    string Password,
    string ConfirmPassword,
    string? DeviceId,
    string? FcmToken,
    NotificationStatusEnum NotificationStatus = NotificationStatusEnum.on
    ) : IRequest<ApiResponse<RegisterCustomerResponse>>;
}
