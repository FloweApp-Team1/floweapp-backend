using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using IdentityService.Common.Extensions;
using IdentityService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registers every IEndpoint implementation found in this assembly
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

// Needed because some stubs already call RequireAuthorization("AdminOnly")
builder.Services.AddAuthentication(); // configure JWT bearer later
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
});

#region Firebase Admin SDK Configuration
//var credentialsPath = builder.Configuration["Firebase:CredentialsPath"]
//    ?? throw new InvalidOperationException("Firebase:CredentialsPath is missing in configuration.");
//var fullPath = Path.Combine(builder.Environment.ContentRootPath, credentialsPath);

//if (!File.Exists(fullPath))
//    throw new FileNotFoundException($"Firebase credentials file not found at '{fullPath}'.");

//builder.Services.AddSingleton(_ => FirebaseApp.Create(new AppOptions
//{
//    Credential = CredentialFactory.FromFile<ServiceAccountCredential>(fullPath).ToGoogleCredential()
//}));
#endregion

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapEndpoints();
app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
