using IdentityService.Domain.Entities;

namespace IdentityService.Common.Interfaces
{
    public interface IJwtService
    {

        string GenerateAccessToken(User user, IEnumerable<string> roles,
            string? driverApplicationStatus = null, Guid? sessionId = null);
        string GenerateRefreshTokenValue();
        string HashRefreshTokenValue(string rawToken);
    }
}
