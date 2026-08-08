using IdentityService.Common.Interfaces;
using IdentityService.Common.Security;
using IdentityService.Common.Settings;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<EmailSettings>(configuration.GetSection("Email"));

            services.AddHttpContextAccessor();

            // Generic Repository + Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Remaining infrastructure services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("Jwt settings are missing in configuration.");

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
                options.AddPolicy(AppPolicies.DriverApproved, p => p.Requirements.Add(new DriverApprovedRequirement()));
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddScoped<IAuthorizationHandler, DriverApprovedHandler>();

            return services;
        }
    }
}
