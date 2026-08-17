using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Persistence;
using Shared.Extensions;

namespace PaymentService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DotNetEnv.Env.TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            
            // Add health checks for docker-compose and API Gateway
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection(); // Removed to prevent internal docker redirects

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapEndpoints();
            app.MapSharedHealthChecks();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                await context.Database.MigrateAsync();
            }

            app.Run();
        }
    }
}
