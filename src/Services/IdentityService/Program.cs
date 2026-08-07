using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using IdentityService.Common.Behaviors;
using IdentityService.Common.Contracts;
using IdentityService.Common.Extensions;
using IdentityService.Common.Handlers;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Settings;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using IdentityService.Infrastructure.Services.Email;
using IdentityService.Infrastructure.Services.OTP;
using IdentityService.Infrastructure.Services.Redis;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Reflection;
using System.Text;


DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);


static string GetRequiredEnv(string key) =>
    Environment.GetEnvironmentVariable(key)
        ?? throw new InvalidOperationException(
            $"Required environment variable '{key}' is not set. Add it to your .env file.");

var connectionString = GetRequiredEnv("ConnectionStrings__DefaultConnection");


builder.Services.AddDbContext<AuthDbContext>(options =>
               options.UseSqlServer(connectionString));

// Handlers depend on IUserRepository, never on AuthDbContext directly.
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Generic repository + unit of work, used by ChangePassword/Sessions/RefreshToken/Logout handlers.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// Ambient access to the authenticated user for the current request.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Access/refresh token issuance for the RefreshToken handler.
// Jwt__* env vars (.env locally, docker-compose `environment:` in containers) bind to Jwt:*.
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey), "Jwt__SecretKey is not set.")
    .Validate(s => Encoding.UTF8.GetByteCount(s.SecretKey ?? "") >= 32, "Jwt__SecretKey must be at least 32 characters.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "Jwt__Issuer is not set.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "Jwt__Audience is not set.")
    .Validate(s => s.AccessTokenExpiryMinutes > 0, "Jwt__AccessTokenExpiryMinutes must be greater than 0.")
    .ValidateOnStart();
builder.Services.AddScoped<IJwtService, JwtService>();


// Turns unhandled exceptions (and FluentValidation failures) into the unified ApiResponse shape
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// MediatR + the FluentValidation pipeline that runs request validators before handlers.
// MediatR.Extensions.Microsoft.DependencyInjection 10.0.1's AddMediatR predates the
// RegisterServicesFromAssembly(cfg) fluent config API - it takes the assembly directly.
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Redis: OTP and reset tokens are stored here (see RedisCacheService).
var redisConnectionString = GetRequiredEnv("Redis__ConnectionString");
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddSingleton<IOtpGenerator, OtpGenerator>();

// OTP pepper: mixed into every OTP hash so a leaked Redis dump alone isn't enough
// to replay codes. Required like the other secrets above; never hardcode this.
var otpPepper = GetRequiredEnv("Otp__PepperSecret");
builder.Services.AddSingleton(new OtpSettings(otpPepper));
builder.Services.AddSingleton<IOtpHasher, OtpHasher>();

builder.Services.AddScoped<IResetTokenService, ResetTokenService>();

builder.Services.AddScoped<IOtpService, OtpService>();

// Transactional email (SMTP via MailKit). Binds Email__* env vars -> EmailSettings.
builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("Email"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.SmtpHost), "Email__SmtpHost is not set.")
    .Validate(s => s.SmtpPort > 0, "Email__SmtpPort is not set.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.SenderEmail), "Email__SenderEmail is not set.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.SenderName), "Email__SenderName is not set.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Username), "Email__Username is not set.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Password), "Email__Password is not set.")
    .ValidateOnStart();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// JWT bearer, wired from the same Jwt:* configuration the token issuer uses.
builder.Services.AddJwtAuthentication(builder.Configuration);
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

builder.Services.AddSingleton(_ => FirebaseApp.Create(new AppOptions
{
    Credential = CredentialFactory.FromFile<ServiceAccountCredential>(fullPath).ToGoogleCredential()
}));

#endregion


builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Endpoints declare their payloads as nested types (e.g. ResetPasswordEndpoint.Request).
    // The default schemaId is the bare type name, so every `Request` collides and swagger.json
    // fails with a 500. Qualify nested types with their declaring type instead.
    options.CustomSchemaIds(SwaggerSchemaId);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token only - Swagger adds the \"Bearer \" prefix."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});

static string SwaggerSchemaId(Type type)
{
    var name = type.Name;

    if (type.IsGenericType)
    {
        name = string.Concat(
            name.AsSpan(0, name.IndexOf('`')),
            "Of",
            string.Join("And", type.GetGenericArguments().Select(SwaggerSchemaId)));
    }

    return type.DeclaringType is null ? name : SwaggerSchemaId(type.DeclaringType) + name;
}


var app = builder.Build();

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
