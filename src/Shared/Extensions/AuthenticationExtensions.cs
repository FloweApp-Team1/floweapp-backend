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
            var jwtSecret = Required(configuration, "Jwt:SecretKey");
            var jwtIssuer = Required(configuration, "Jwt:Issuer");
            var jwtAudience = Required(configuration, "Jwt:Audience");

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

        private static string Required(IConfiguration configuration, string key) =>
            configuration[key] is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException(
                    $"Required configuration '{key}' is not set. Add '{key.Replace(":", "__")}' to your .env file.");
    }
}
