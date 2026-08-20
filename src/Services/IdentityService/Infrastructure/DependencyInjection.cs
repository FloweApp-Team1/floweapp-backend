using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Shared.Behaviors;
using IdentityService.Common.Contracts;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Handlers;
using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using Shared.Security;
using Shared.Settings;
using Shared.Swagger;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using IdentityService.Infrastructure.Services.Email;
using IdentityService.Infrastructure.Services.OTP;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using MassTransit;

namespace IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        private static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(ApplicationAssembly));

            services.AddValidatorsFromAssembly(ApplicationAssembly);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Registers every IEndpoint implementation; MapEndpoints() maps them.
            services.AddEndpoints(ApplicationAssembly);

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // Renders 401/403 in the same ApiResponse shape as every other failure.
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();

            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddConfigurationOptions(configuration);

            var connectionString = Required(configuration, "ConnectionStrings:AuthDatabase");

            services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));

            // Persistence
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();

            // Security / identity
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Messaging / storage
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IImageService, LocalImageService>();

            services.AddSharedRedis(configuration);
            services.AddOtpServices();
            services.AddFirebase(configuration, environment);

            return services;
        }

        private static IServiceCollection AddConfigurationOptions(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("Jwt"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey), "Jwt__SecretKey is not set.")
                .Validate(s => Encoding.UTF8.GetByteCount(s.SecretKey ?? "") >= 32, "Jwt__SecretKey must be at least 32 characters.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "Jwt__Issuer is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "Jwt__Audience is not set.")
                .Validate(s => s.AccessTokenExpiryMinutes > 0, "Jwt__AccessTokenExpiryMinutes must be greater than 0.")
                .Validate(s => s.RefreshTokenExpiryDays > 0, "Jwt__RefreshTokenExpiryDays must be greater than 0.")
                .ValidateOnStart();

            services.AddOptions<EmailSettings>()
                .Bind(configuration.GetSection("Email"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.SmtpHost), "Email__SmtpHost is not set.")
                .Validate(s => s.SmtpPort > 0, "Email__SmtpPort is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.SenderEmail), "Email__SenderEmail is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.SenderName), "Email__SenderName is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Username), "Email__Username is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Password), "Email__Password is not set.")
                .ValidateOnStart();

            return services;
        }


        private static IServiceCollection AddOtpServices(this IServiceCollection services)
        {
            services.AddSingleton<IOtpGenerator, OtpGenerator>();

            // Pepper mixed into every OTP hash so a leaked Redis dump alone isn't
            // enough to replay codes.
            services.AddSingleton(sp => new OtpSettings(
                Required(sp.GetRequiredService<IConfiguration>(), "Otp:PepperSecret")));
            services.AddSingleton<IOtpHasher, OtpHasher>();

            services.AddScoped<IOtpService, OtpService>();

            services.AddScoped<IResetTokenService, ResetTokenService>();

            return services;
        }

        private static IServiceCollection AddFirebase(
            this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var credentialsPath = Required(configuration, "Firebase:CredentialsPath");

            var fullPath = Path.IsPathRooted(credentialsPath)
                ? credentialsPath
                : Path.Combine(environment.ContentRootPath, credentialsPath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Firebase credentials file not found at '{fullPath}'.");

            services.AddSingleton(_ => FirebaseApp.Create(new AppOptions
            {
                Credential = CredentialFactory.FromFile<ServiceAccountCredential>(fullPath).ToGoogleCredential()
            }));

            return services;
        }

        // The single authentication + authorization configuration.
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSharedJwtAuthentication(configuration);

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

        // Per-IP throttle for the admin login endpoint (.RequireRateLimiting("AdminLoginPerIp")).
        public static IServiceCollection AddAdminLoginRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddPolicy(RateLimitPolicies.AdminLoginPerIp, httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(15),
                            SegmentsPerWindow = 3,
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, ct) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests from this IP. Please try again later.", ct);
                };
            });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(SwaggerSchemaId);
                options.ParameterFilter<EnumParameterFilter>();

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the access token only - Swagger adds the \"Bearer \" prefix."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    }] = Array.Empty<string>()
                });
            });

            return services;
        }


        private static string SwaggerSchemaId(Type type)
        {
            var name = type.Name;

            if (type.IsGenericType)
            {
                name = string.Concat(
                    name.AsSpan(0, name.IndexOf('`')),
                    "Of",
                    string.Join("And", type.GetGenericArguments().Select(SwaggerSchemaId)));
            }

            return type.DeclaringType is null ? name : SwaggerSchemaId(type.DeclaringType) + name;
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
