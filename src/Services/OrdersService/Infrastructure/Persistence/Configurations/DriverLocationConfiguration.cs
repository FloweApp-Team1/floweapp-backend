using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations
{
    public class DriverLocationConfiguration : IEntityTypeConfiguration<DriverLocation>
    {
        public void Configure(EntityTypeBuilder<DriverLocation> builder)
        {
            builder.ToTable("DriverLocations");

            builder.HasKey(x => x.Id);

            // One current-location row per order - see DriverLocation's remarks.
            builder.HasIndex(x => x.OrderId).IsUnique();
            builder.HasIndex(x => x.DriverId);

            builder.Property(x => x.RecordedAt).IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
