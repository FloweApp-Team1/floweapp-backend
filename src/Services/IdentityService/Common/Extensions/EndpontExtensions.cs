using IdentityService.Common.Contracts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace IdentityService.Common.Extensions
{
    public static class EndpointExtensions
    {
        // Scans the assembly and registers every class implementing IEndpoint
        public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
        {
            var descriptors = assembly.DefinedTypes
                .Where(t => t is { IsAbstract: false, IsInterface: false }
                            && t.IsAssignableTo(typeof(IEndpoint)))
                .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t));

            services.TryAddEnumerable(descriptors);
            return services;
        }

        // Resolves them all and calls MapEndpoint on each
        public static WebApplication MapEndpoints(this WebApplication app)
        {
            var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();


            foreach (var endpoint in endpoints)
                endpoint.MapEndpoint(app);

            return app;
        }
    }
    }
