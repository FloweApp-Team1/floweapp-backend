using IdentityService.Common.Contracts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace IdentityService.Infrastructure.Services
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId =>
            Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id
                : null;

        public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
    }
}
