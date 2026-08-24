using AddressCartService.Infrastructure.Messaging;
using AddressCartService.Infrastructure.Persistence;
using AddressCartService.Infrastructure.Repositories;
using AddressCartService.Infrastructure.Services;
using AddressCartService.Infrastructure.Services.Catalog;
using AddressCartService.Infrastructure.Services.Geocoding;
using AddressCartService.Infrastructure.Services.StoreCoverage;
using AddressCartService.Infrastructure.Settings;
using MassTransit;
using Shared.Behaviors;
using Shared.Extensions;
using Shared.Handlers;
using Shared.Interfaces;
using Shared.Security;
using Shared.Settings;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace AddressCartService.Infrastructure
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

            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddConfigurationOptions(services, configuration);

            var connectionString = Required(configuration, "ConnectionStrings:AddressCartDatabase");
            services.AddDbContext<AddressCartDbContext>(options => options.UseSqlServer(connectionString));

            // Persistence
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ILocationRepository, LocationRepository>();

            services.AddSharedRedis(configuration);

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IStoreResolutionService, StoreResolutionService>();

            // Geocoding__UseMockProvider swaps in a canned, no-network implementation for
            // local/manual testing - read directly since the provider choice is made at
            // registration time, before the options pipeline resolves GeocodingSettings.
            if (configuration.GetValue<bool>("Geocoding:UseMockProvider"))
            {
                services.AddScoped<IGeocodingProvider, MockGeocodingProvider>();
            }
            else
            {
                // Typed HttpClient for the reverse-geocoding provider; BaseAddress isn't set
                // here since GeocodingSettings.BaseUrl is only resolvable once options are bound.
                services.AddHttpClient<IGeocodingProvider, GoogleGeocodingProvider>((provider, client) =>
                {
                    var settings = provider.GetRequiredService<IOptions<GeocodingSettings>>().Value;
                    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                });
            }

            // ICatalogClient HTTP registration
            services.AddHttpClient<ICatalogClient, CatalogClient>();

            AddIntegrationEventPublisher(services, configuration);

            return services;
        }

        private static void AddIntegrationEventPublisher(IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMq = configuration.GetSection(AddressCartService.Infrastructure.Messaging.RabbitMqSettings.SectionName)
                .Get<AddressCartService.Infrastructure.Messaging.RabbitMqSettings>();

            if (rabbitMq is null || string.IsNullOrWhiteSpace(rabbitMq.Host))
            {
                services.AddSingleton<IIntegrationEventPublisher, LoggingEventPublisher>();
                return;
            }

            services.AddMassTransit(bus =>
            {
                bus.SetKebabCaseEndpointNameFormatter();
                bus.AddConsumers(Assembly.GetExecutingAssembly());

                bus.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(rabbitMq.Host, host =>
                    {
                        host.Username(rabbitMq.Username);
                        host.Password(rabbitMq.Password);
                    });

                    configurator.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();
        }

        private static void AddConfigurationOptions(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<CatalogSettings>()
                .Bind(configuration.GetSection(CatalogSettings.SectionName))
                .Validate(s => !string.IsNullOrWhiteSpace(s.BaseUrl), "CatalogService__BaseUrl is not set.")
                .ValidateOnStart();

            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("Jwt"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey), "Jwt__SecretKey is not set.")
                .Validate(s => Encoding.UTF8.GetByteCount(s.SecretKey ?? "") >= 32, "Jwt__SecretKey must be at least 32 characters.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "Jwt__Issuer is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "Jwt__Audience is not set.")
                .ValidateOnStart();

            services.AddOptions<GeocodingSettings>()
                .Bind(configuration.GetSection("Geocoding"))
                .Validate(s => s.UseMockProvider || !string.IsNullOrWhiteSpace(s.ApiKey), "Geocoding__ApiKey is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.BaseUrl), "Geocoding__BaseUrl is not set.")
                .ValidateOnStart();
        }
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSharedJwtAuthentication(configuration);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AppPolicies.AdminOnly, p => p.RequireRole(AppRoles.Admin));
                options.AddPolicy(AppPolicies.CustomerOnly, p => p.RequireRole(AppRoles.Customer));
                options.AddPolicy(AppPolicies.DriverOnly, p => p.RequireRole(AppRoles.Driver));

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
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
        private static string Required(IConfiguration configuration, string key) =>
            configuration[key] is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException(
                    $"Required configuration '{key}' is not set. Add '{key.Replace(":", "__")}' to your .env file.");
    }
}
