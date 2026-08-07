using IdentityService.Common.Models;
using MediatR;

namespace IdentityService.Features.Auth.Login
{
    public record LoginCommand(LoginRequest Request, /* string AppType, */ string? IpAddress) : IRequest<Result<AuthResponse>>;
}
