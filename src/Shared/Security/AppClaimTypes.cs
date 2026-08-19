using System.Security.Claims;

namespace Shared.Security
{

    public static class AppClaimTypes
    {
        public const string Role = ClaimTypes.Role;          // reuse the standard claim
        public const string ApplicationStatus = "applicationStatus"; // Driver-only claim

        // Identity carried on the token itself. OrdersService copies these onto an order
        // when a driver claims it, so the tracking screen's driver card costs no
        // cross-service call and survives IdentityService being down.
        public const string FirstName = "firstName";
        public const string LastName = "lastName";

        // Not issued today - JwtService only adds the two names above. They are read
        // opportunistically, so adding them when a driver token is minted is enough to
        // complete the driver card; nothing on the consuming side has to change.
        public const string PhoneNumber = "phoneNumber";
        public const string ImageUrl = "imageUrl";
    }

}
