using IdentityService.Domain.Entities;

namespace IdentityService.Common.Security
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, TimeSpan expiresIn);
        string GenerateRefreshTokenValue();
    }
}
