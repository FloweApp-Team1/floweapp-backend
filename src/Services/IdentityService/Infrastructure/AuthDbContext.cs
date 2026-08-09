using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MimeKit.Encodings;
using System.Reflection.Emit;

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
        public DbSet<Guest> Guests { get; set; }
        public DbSet<AdminLoginAudit> AdminLoginAudits { get; set; }
        public DbSet<DriverApplication> DriverApplications { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        override protected void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
.HasQueryFilter(x => !x.IsDeleted);
            base.OnModelCreating(builder);

            builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.User.IsDeleted);
            builder.Entity<UserRole>().HasQueryFilter(ur => !ur.User.IsDeleted);
            builder.Entity<VehicleInfo>().HasQueryFilter(v => !v.Delivery.IsDeleted);

            builder.HasDefaultSchema("Auth");

            builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        }
    }
}
