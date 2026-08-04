using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure
{
    public class AuthDbContext:DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<VehicleInfo> VehicleInfos { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        public DbSet<AdminLoginAudit> AdminLoginAudits { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        override protected void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // This service owns the "Auth" schema inside the shared FloweApp database.
            // Other microservices will use their own schemas in the same database.
            builder.HasDefaultSchema("Auth");

            builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        }
    }
}
