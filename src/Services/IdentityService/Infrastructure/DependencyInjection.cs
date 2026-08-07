using IdentityService.Common.Contracts;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Settings;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using IdentityService.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            // باقي الـ Services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IEmailService, SmtpEmailService>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Jwt__SecretKey / Jwt__Issuer / Jwt__Audience come from the environment
            // (.env locally, docker-compose `environment:` in containers).
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "Jwt settings are missing in configuration. Set Jwt__SecretKey, Jwt__Issuer and Jwt__Audience in your .env file.");

            foreach (var (key, value) in new[]
                     {
                         ("Jwt__SecretKey", jwtSettings.SecretKey),
                         ("Jwt__Issuer", jwtSettings.Issuer),
                         ("Jwt__Audience", jwtSettings.Audience)
                     })
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidOperationException($"'{key}' is not set. Add it to your .env file.");
            }

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
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
