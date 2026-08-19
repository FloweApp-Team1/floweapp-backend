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

            // Lets the tracking endpoint answer "is this position stale?" straight from the
            // index, and keeps the broadcast throttle a single-row read.
            builder.HasIndex(x => x.RecordedAt);

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
