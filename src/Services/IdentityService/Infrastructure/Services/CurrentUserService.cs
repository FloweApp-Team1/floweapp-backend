using Shared.Interfaces;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityService.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var value = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                            ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        public string? Email
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirstValue(JwtRegisteredClaimNames.Email)
                       ?? user?.FindFirstValue(ClaimTypes.Email);
            }
        }

        public string? IpAddress
            => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? DeviceName
            => _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString();
    }
}
