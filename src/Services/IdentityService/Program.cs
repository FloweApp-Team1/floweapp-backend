using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using IdentityService.Common.Behaviors;
using IdentityService.Common.Extensions;
using IdentityService.Common.Handlers;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Security;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using MediatR;
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

//Services Registeration
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Turns unhandled exceptions (and FluentValidation failures) into the unified ApiResponse shape
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Registers JWT bearer authentication plus AdminOnly/CustomerOnly/DriverOnly/DriverApproved policies
builder.Services.AddJwtAuthentication(builder.Configuration);

#region Firebase Admin SDK Configuration
var credentialsPath = GetRequiredEnv("Firebase__CredentialsPath");

var fullPath = Path.IsPathRooted(credentialsPath)
    ? credentialsPath
    : Path.Combine(builder.Environment.ContentRootPath, credentialsPath);

if (!File.Exists(fullPath))
    throw new FileNotFoundException($"Firebase credentials file not found at '{fullPath}'.");
#endregion

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


// Registers IUnitOfWork, IGenericRepository<>, IJwtService, IEmailService,
// ICurrentUserService, IHttpContextAccessor, and binds JwtSettings/EmailSettings
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();


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

app.UseAuthentication();
app.UseAuthorization();

// Maps every IEndpoint feature (Auth, Users, Drivers, Vehicles, Admin, ...)
app.MapEndpoints();

app.Run();
