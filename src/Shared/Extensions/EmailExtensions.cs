namespace Shared.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.Contracts;
    using Shared.Infrastructure.Services.Email;

    public static class EmailExtensions
    {
        public static IServiceCollection AddSharedEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<EmailSettings>()
                .Bind(configuration.GetSection("Email"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.SmtpHost), "Email__SmtpHost is not set.")
                .Validate(s => s.SmtpPort > 0, "Email__SmtpPort must be greater than zero.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.SenderEmail), "Email__SenderEmail is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.SenderName), "Email__SenderName is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Username), "Email__Username is not set.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Password), "Email__Password is not set.")
                .ValidateOnStart();

            services.AddScoped<IEmailService, SmtpEmailService>();

            return services;
        }
    }
}
