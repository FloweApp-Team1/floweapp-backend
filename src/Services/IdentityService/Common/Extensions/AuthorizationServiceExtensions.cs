namespace IdentityService.Common.Extensions
{
    public static class AuthorizationServiceExtensions
    {
        public static IServiceCollection AddAdminAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            return services;
        }
    }
}
