namespace ApiGateway.Configuration
{
    // Bound from the Gateway__* environment variables (.env locally, docker-compose
    // `environment:` in containers) - never from appsettings.json, so a missing value
    // fails startup instead of being shadowed by a placeholder default.
    public class GatewaySettings
    {
        public const string SectionName = "Gateway";

        public string IdentityServiceUrl { get; set; } = null!;
        public string CatalogServiceUrl { get; set; } = null!;

        // Comma-separated list of browser origins, or "*" for any origin.
        public string AllowedOrigins { get; set; } = null!;

        public RateLimitingSettings RateLimiting { get; set; } = new();

        public HealthCheckSettings HealthCheck { get; set; } = new();

        public bool AllowAnyOrigin => AllowedOrigins.Trim() == "*";

        public string[] Origins =>
            AllowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Binds the section and fails fast with the exact variable name to add, matching
        // how the services validate their own configuration.
        public static GatewaySettings Bind(IConfiguration configuration)
        {
            var settings = configuration.GetSection(SectionName).Get<GatewaySettings>() ?? new GatewaySettings();

            Required(settings.IdentityServiceUrl, "Gateway__IdentityServiceUrl");
            Required(settings.CatalogServiceUrl, "Gateway__CatalogServiceUrl");
            Required(settings.AllowedOrigins, "Gateway__AllowedOrigins");

            settings.RateLimiting.Auth.Validate("Gateway__RateLimiting__Auth");
            settings.RateLimiting.Default.Validate("Gateway__RateLimiting__Default");

            settings.HealthCheck.Validate("Gateway__HealthCheck");

            return settings;
        }

        private static void Required(string? value, string variable)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    $"Required configuration '{variable}' is not set. Add it to your .env file.");
        }
    }

    public class RateLimitingSettings
    {
        // Applied to the identity routes (login, register, OTP) - deliberately tighter.
        public RateLimitWindow Auth { get; set; } = new();

        // Applied to every other proxied route.
        public RateLimitWindow Default { get; set; } = new();
    }

    public class RateLimitWindow
    {
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }

        public void Validate(string prefix)
        {
            if (PermitLimit <= 0)
                throw new InvalidOperationException(
                    $"Required configuration '{prefix}__PermitLimit' is not set or is not greater than zero. Add it to your .env file.");

            if (WindowSeconds <= 0)
                throw new InvalidOperationException(
                    $"Required configuration '{prefix}__WindowSeconds' is not set or is not greater than zero. Add it to your .env file.");
        }
    }

    public class HealthCheckSettings
    {
        public int IntervalSeconds { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 5;

        public void Validate(string prefix)
        {
            if (IntervalSeconds <= 0)
                throw new InvalidOperationException(
                    $"Required configuration '{prefix}__IntervalSeconds' is not set or is not greater than zero. Add it to your .env file.");

            if (TimeoutSeconds <= 0)
                throw new InvalidOperationException(
                    $"Required configuration '{prefix}__TimeoutSeconds' is not set or is not greater than zero. Add it to your .env file.");
        }
    }
}
