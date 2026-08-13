using Microsoft.AspNetCore.Authorization;


namespace Shared.Security
{
    // Distinguishes "wrong role" from "right role, not approved yet" so the
    // result handler below can return a different message for each (criteria 4).
    public class DriverApprovedHandler : AuthorizationHandler<DriverApprovedRequirement>
    {
        public const string WrongRoleReason = "wrong_role";
        public const string NotApprovedReason = "not_approved";

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, DriverApprovedRequirement requirement)
        {
            if (!context.User.IsInRole(AppRoles.Driver))
            {
                context.Fail(new AuthorizationFailureReason(this, WrongRoleReason));
                return Task.CompletedTask;
            }

            var status = context.User.FindFirst(AppClaimTypes.ApplicationStatus)?.Value;

            if (!string.Equals(status, "APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                context.Fail(new AuthorizationFailureReason(this, NotApprovedReason));
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
