using IdentityService.Common.Models;
using MediatR;

namespace IdentityService.Features.Auth.Login
{
    public record LoginCommand(LoginRequest Request, string AppType) : IRequest<Result<AuthResponse>>;
}
