using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrdersService.Features.Checkout.Common;
using OrdersService.Infrastructure.Clients;
using OrdersService.Infrastructure.Messaging;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Repositories;
using OrdersService.Infrastructure.Services;
using OrdersService.Infrastructure.Settings;
using Polly;
using Polly.Extensions.Http;
using Shared.Behaviors;
using Shared.Extensions;
using Shared.Handlers;
using Shared.Interfaces;
using Shared.Security;
using Shared.Settings;
using System.Reflection;
using System.Text;

namespace OrdersService.Infrastructure
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

            var connectionString = Required(configuration, "ConnectionStrings:OrdersDatabase");
            services.AddDbContext<OrdersDbContext>(options => options.UseSqlServer(connectionString));

            // Persistence
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IOrderStatusHistoryWriter, OrderStatusHistoryWriter>();
            services.AddScoped<IDriverSnapshotService, DriverSnapshotService>();

            services.AddSharedEmailService(configuration);

            AddDriverLocationCache(services, configuration);
            AddIntegrationEventPublisher(services, configuration);
            AddHttpClients(services, configuration);

            services.AddScoped<ICatalogServiceClient, CatalogServiceClient>();
            services.AddScoped<IAddressServiceClient, HttpAddressServiceClient>();
            services.AddScoped<ICartServiceClient, HttpCartServiceClient>();
            services.AddScoped<IDeliveryFeeCalculator, FlatRateDeliveryFeeCalculator>();
            services.AddScoped<IPaymentMethodProvider, StaticPaymentMethodProvider>();
            services.AddScoped<IDeliveryEstimateCalculator, FlatDeliveryEstimateCalculator>();
            services.AddScoped<ICheckoutPricingService, CheckoutPricingService>();
            services.AddScoped<IIdempotencyService, DistributedCacheIdempotencyService>();

            services.AddTransient<AuthorizationHeaderForwardingHandler>();


            return services;
        }

        private static void AddDriverLocationCache(IServiceCollection services, IConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration["Redis:ConnectionString"]))
            {
                services.AddSingleton<IDriverLocationCache, NullDriverLocationCache>();
                return;
            }

            services.AddSharedRedis(configuration);
            services.AddScoped<IDriverLocationCache, DriverLocationCache>();
        }

        private static void AddHttpClients(IServiceCollection services, IConfiguration configuration)
        {
            var paymentUrl = Required(configuration, "Gateway:PaymentServiceUrl");
            var catalogUrl = Required(configuration, "Gateway:CatalogServiceUrl");
            var addressCartUrl = Required(configuration, "Gateway:AddressCartServiceUrl");

            services.AddHttpClient("PaymentServiceClient", client =>
            {
                client.BaseAddress = new Uri(paymentUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("CatalogServiceClient", client =>
            {
                client.BaseAddress = new Uri(catalogUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("AddressCartServiceClient", client =>
            {
                client.BaseAddress = new Uri(addressCartUrl);
            })
            .AddHttpMessageHandler<AuthorizationHeaderForwardingHandler>();

        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        // Same reasoning for RabbitMQ: a missing broker costs the customer the silent push
        // that moves the map marker, and polling still carries the tracking screen.
        private static void AddIntegrationEventPublisher(IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMq = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();

            if (rabbitMq is null || string.IsNullOrWhiteSpace(rabbitMq.Host))
            {
                services.AddSingleton<IIntegrationEventPublisher, LoggingEventPublisher>();
                return;
            }

            services.AddMassTransit(bus =>
            {
                bus.SetKebabCaseEndpointNameFormatter();
                bus.AddConsumers(Assembly.GetExecutingAssembly());

                bus.AddEntityFrameworkOutbox<OrdersDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

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
            services.AddOptions<DeliveryTrackingSettings>()
                .Bind(configuration.GetSection(DeliveryTrackingSettings.SectionName))
                .Validate(s => s.StalenessThresholdSeconds > 0, "DeliveryTracking__StalenessThresholdSeconds must be greater than zero.")
                .Validate(s => s.CacheTtlMinutes > 0, "DeliveryTracking__CacheTtlMinutes must be greater than zero.")
                .Validate(s => s.MinimumBroadcastDistanceMeters >= 0, "DeliveryTracking__MinimumBroadcastDistanceMeters cannot be negative.")
                .Validate(s => s.MinimumBroadcastIntervalSeconds >= 0, "DeliveryTracking__MinimumBroadcastIntervalSeconds cannot be negative.")
                .Validate(s => s.MaximumPingAgeMinutes > 0, "DeliveryTracking__MaximumPingAgeMinutes must be greater than zero.")
                .ValidateOnStart();

            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("Jwt"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey), "Jwt__SecretKey is not set.")
                .Validate(s => Encoding.UTF8.GetByteCount(s.SecretKey ?? "") >= 32, "Jwt__SecretKey must be at least 32 characters.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "Jwt__Issuer is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "Jwt__Audience is not set.")
                .ValidateOnStart();
        }

        // Validates tokens issued by IdentityService - this service never issues its own,
        // so it must share the same signing key/issuer/audience as IdentityService's Jwt__* config.
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

            // Same claims-only requirement IdentityService evaluates at login - no DB call,
            // so it's safe to reuse here against the JWT's own applicationStatus claim.
            services.AddScoped<IAuthorizationHandler, DriverApprovedHandler>();

            // Renders 401/403 in the same ApiResponse shape as every other failure.
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

        // Configuration comes from environment variables only (.env locally,
        // docker-compose `environment:` in containers) - never from appsettings.json.
        private static string Required(IConfiguration configuration, string key) =>
            configuration[key] is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException(
                    $"Required configuration '{key}' is not set. Add '{key.Replace(":", "__")}' to your .env file.");
    }
}
