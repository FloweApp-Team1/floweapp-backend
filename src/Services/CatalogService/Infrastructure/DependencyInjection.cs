using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Services;
using Shared.Interfaces;
using Shared.Security;
using Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CatalogService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddConfigurationOptions(services, configuration);

            var connectionString = Required(configuration, "ConnectionStrings:CatalogDatabase");
            services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));

            // Persistence
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }

        private static void AddConfigurationOptions(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("Jwt"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey), "Jwt__SecretKey is not set.")
                .Validate(s => Encoding.UTF8.GetByteCount(s.SecretKey ?? "") >= 32, "Jwt__SecretKey must be at least 32 characters.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "Jwt__Issuer is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "Jwt__Audience is not set.")
                .ValidateOnStart();
        }

        // Validates tokens issued by IdentityService - Catalog never issues its own,
        // so this must share the same signing key/issuer/audience as IdentityService's Jwt__* config.
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "Jwt settings are missing. Set Jwt__SecretKey, Jwt__Issuer and Jwt__Audience in your .env file.");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AppPolicies.AdminOnly, p => p.RequireRole(AppRoles.Admin));
                options.AddPolicy(AppPolicies.CustomerOnly, p => p.RequireRole(AppRoles.Customer));
                options.AddPolicy(AppPolicies.DriverOnly, p => p.RequireRole(AppRoles.Driver));

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Renders 401/403 in the same ApiResponse shape as every other failure.
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();

            return services;
        }

        // Configuration comes from environment variables only (.env locally,
        // docker-compose `environment:` in containers) - never from appsettings.json.
        private static string Required(IConfiguration configuration, string key) =>
            configuration[key] is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException(
                    $"Required configuration '{key}' is not set. Add '{key.Replace(':', '_').Replace("_", "__")}' to your .env file.");
    }
}
