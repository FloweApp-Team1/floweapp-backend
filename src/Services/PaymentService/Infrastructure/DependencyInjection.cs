using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Persistence;
using Shared.Behaviors;
using Shared.Extensions;
using System.Reflection;
using MassTransit;

namespace PaymentService.Infrastructure
{
    public static class DependencyInjection
    {
        private static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(ApplicationAssembly));

            services.AddValidatorsFromAssembly(ApplicationAssembly);
            services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddEndpoints(ApplicationAssembly);

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            Stripe.StripeConfiguration.ApiKey = Required(configuration, "Stripe:SecretKey");

            var connectionString = Required(configuration, "ConnectionStrings:PaymentDatabase");
            services.AddDbContext<PaymentDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<Shared.Interfaces.IUnitOfWork, Repositories.UnitOfWork>();
            services.AddScoped(typeof(Shared.Interfaces.IGenericRepository<>), typeof(Repositories.GenericRepository<>));

            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RABBITMQ_HOST"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RABBITMQ_USER"] ?? "guest");
                        h.Password(configuration["RABBITMQ_PASSWORD"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
            
            services.AddHttpContextAccessor();

            return services;
        }

        private static string Required(IConfiguration configuration, string key) =>
            configuration[key] is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException(
                    $"Required configuration '{key}' is not set. Add '{key.Replace(':', '_').Replace("_", "__")}' to your .env file.");
    }
}
