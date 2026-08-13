using System.Security.Claims;

namespace Shared.Security
{

    public static class AppClaimTypes
    {
        public const string Role = ClaimTypes.Role;          // reuse the standard claim
        public const string ApplicationStatus = "applicationStatus"; // Driver-only claim
    }

}
