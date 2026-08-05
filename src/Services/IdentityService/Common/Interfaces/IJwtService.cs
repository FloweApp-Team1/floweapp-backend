using IdentityService.Domain.Entities;

namespace IdentityService.Common.Interfaces
{
    public interface IJwtService
    {

        string GenerateAccessToken(User user, IEnumerable<string> roles, string? driverApplicationStatus = null);
        string GenerateRefreshTokenValue();
    }
}
