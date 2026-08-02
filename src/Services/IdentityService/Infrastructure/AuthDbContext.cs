using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Dilvery> Deliveries { get; set; }
        public DbSet<VehicleInfo> vehicleInfos { get; set; }
        public DbSet<OtpCode> otpCodes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        override protected void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(e =>
            {
                e.Property(x => x.FirstName).HasMaxLength(50);
                e.Property(x => x.LastName).HasMaxLength(50);
                e.Property(x => x.Gender).IsRequired();
                e.Property(x => x.CreatedAt).IsRequired();

            });
            builder.Entity<Customer>(e =>
            {
                e.HasKey(e => e.Id);
                e.Property(e => e.Address).IsRequired();
                e.HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Customer>(c => c.UserId);
            });




            builder.Entity<Dilvery>(e =>
            {
                e.HasKey(e => e.Id);
                e.Property(e => e.NationalIdNumber).IsRequired();
                e.Property(e => e.VehiclePlateNumber).IsRequired();
                e.Property(e => e.LicenseDocument).IsRequired();

                e.HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Dilvery>(d => d.UserId);

                e.HasOne(d => d.VehicleInfo)
                .WithOne()
                .HasForeignKey<Dilvery>(d => d.VehicleId);
            });




        }
    }
}