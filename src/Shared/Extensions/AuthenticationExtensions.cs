using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Shared.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddSharedJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecret = configuration["Jwt:SecretKey"] ?? configuration["Jwt__SecretKey"];
            var jwtIssuer = configuration["Jwt:Issuer"] ?? configuration["Jwt__Issuer"];
            var jwtAudience = configuration["Jwt:Audience"] ?? configuration["Jwt__Audience"];

            if (string.IsNullOrEmpty(jwtSecret))
            {
                // In some environments, this might be intentional (e.g. tests), so we return early
                // or we could throw an exception if strict checking is needed.
                return services;
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}
