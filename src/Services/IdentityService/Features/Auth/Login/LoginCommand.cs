using Shared.Models;
using Shared.Results;
using MediatR;

namespace IdentityService.Features.Auth.Login
{
    public record LoginCommand(LoginRequest Request, string? IpAddress) : IRequest<Result<AuthResponse>>;
}
