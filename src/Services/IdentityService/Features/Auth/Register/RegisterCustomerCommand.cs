using IdentityService.Common.Responses;
using IdentityService.Domain.Enums;
using MediatR;

namespace IdentityService.Features.Auth.Register
{
    public record RegisterCustomerCommand(
    string FullName,
    string Email,
    string Phone,
    GenderEnum Gender,
    string Password,
    string ConfirmPassword,
    string FcmToken,
    NotifcationStatusEnum NotificationStatus = NotifcationStatusEnum.on
    ) : IRequest<ApiResponse<RegisterCustomerResponse>>;
}
