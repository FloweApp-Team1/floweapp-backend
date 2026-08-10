using Shared.Responses;
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
    NotificationStatusEnum NotificationStatus = NotificationStatusEnum.on
    ) : IRequest<ApiResponse<RegisterCustomerResponse>>;
}
