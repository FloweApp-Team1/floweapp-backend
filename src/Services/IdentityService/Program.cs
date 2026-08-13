using Shared.Contracts;
using Shared.Extensions;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;


DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAdminLoginRateLimiting();
builder.Services.AddSwaggerDocumentation();

// Backs GET /health, which docker-compose probes before letting the gateway start.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuthDbContext>("database");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Removed to prevent internal docker redirects

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapSharedHealthChecks();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    await context.Database.MigrateAsync();

    await AdminSeeder.SeedAsync(context, configuration, passwordHasher);
}

app.Run();
