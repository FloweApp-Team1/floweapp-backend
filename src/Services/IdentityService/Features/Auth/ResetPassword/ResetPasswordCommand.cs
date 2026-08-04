using IdentityService.Common.Responses;
using MediatR;

namespace IdentityService.Features.Auth.ResetPassword
{
    public sealed record ResetPasswordCommand(
        string OtpToken,
        string Password,
        string ConfirmPassword) : IRequest<ApiResponse<bool>>;
}
