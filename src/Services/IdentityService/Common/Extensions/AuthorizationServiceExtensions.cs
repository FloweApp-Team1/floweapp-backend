using IdentityService.Common.Security;

namespace IdentityService.Common.Extensions
{
    public static class AuthorizationServiceExtensions
    {
        public static IServiceCollection AddAdminAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.AdminOnlyPolicy, policy => policy.RequireRole(RoleConstants.Admin));
            });

            return services;
        }
    }
}
