using AddressCartService.Infrastructure;
using AddressCartService.Infrastructure.Persistence;
using Shared.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Enums serialize as their contract names (e.g. CityAreaList -> "CITY_AREA_LIST"),
// matching the SCREAMING_SNAKE_CASE enum values in the API contract.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));

// Backs GET /health, which docker-compose probes before letting the gateway start.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AddressCartDbContext>("database");

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

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapSharedHealthChecks();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AddressCartDbContext>();

    await context.Database.MigrateAsync();
    await AddressCartService.Infrastructure.Persistence.Seed.LocationSeeder.SeedAsync(context);
}

app.Run();
