using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Persistence;
using Shared.Behaviors;
using Shared.Extensions;
using System.Reflection;

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
            var connectionString = Required(configuration, "ConnectionStrings:PaymentDatabase");
            services.AddDbContext<PaymentDbContext>(options => options.UseSqlServer(connectionString));

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
