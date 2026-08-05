using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using IdentityService.Common.Extensions;
using IdentityService.Common.Handlers;
using IdentityService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);


static string GetRequiredEnv(string key) =>
    Environment.GetEnvironmentVariable(key)
        ?? throw new InvalidOperationException(
            $"Required environment variable '{key}' is not set. Add it to your .env file.");

var connectionString = GetRequiredEnv("ConnectionStrings__DefaultConnection");


builder.Services.AddDbContext<AuthDbContext>(options =>
               options.UseSqlServer(connectionString));

// Registers every IEndpoint implementation found in this assembly
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

// Turns unhandled exceptions (and FluentValidation failures) into the unified ApiResponse shape
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Needed because some stubs already call RequireAuthorization("AdminOnly")
builder.Services.AddAuthentication(); // configure JWT bearer later
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
});

#region Firebase Admin SDK Configuration
var credentialsPath = GetRequiredEnv("Firebase__CredentialsPath");

var fullPath = Path.IsPathRooted(credentialsPath)
    ? credentialsPath
    : Path.Combine(builder.Environment.ContentRootPath, credentialsPath);

if (!File.Exists(fullPath))
    throw new FileNotFoundException($"Firebase credentials file not found at '{fullPath}'.");
#endregion

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(_ => FirebaseApp.Create(new AppOptions
{
    Credential = CredentialFactory.FromFile<ServiceAccountCredential>(fullPath).ToGoogleCredential()
}));


var app = builder.Build();

// Must be first so it can catch exceptions thrown anywhere downstream.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Maps every IEndpoint feature (Auth, Users, Drivers, Vehicles, Admin, ...)
app.MapEndpoints();

app.Run();
