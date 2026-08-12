using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;

namespace CatalogService
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
            builder.Services.AddJwtAuthentication(builder.Configuration);

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

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

                await context.Database.MigrateAsync();

                // Sample catalog is development-only - production data comes from
                // the Admin endpoints.
                if (app.Environment.IsDevelopment())
                    await CatalogSeeder.SeedAsync(context);
            }

            app.Run();
        }
    }
}
