using Shared.Responses;
using MediatR;

namespace IdentityService.Features.Auth.ForgetPassword
{
    public record ForgetPasswordCommand(string email) : IRequest<ApiResponse<bool>>;

}
