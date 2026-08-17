using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations
{
    public class OrderAddressSnapshotConfiguration : IEntityTypeConfiguration<OrderAddressSnapshot>
    {
        public void Configure(EntityTypeBuilder<OrderAddressSnapshot> builder)
        {
            builder.ToTable("OrderAddressSnapshots");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RecipientName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.RecipientPhone).IsRequired().HasMaxLength(20);
            builder.Property(x => x.AddressLine).IsRequired().HasMaxLength(500);
            builder.Property(x => x.City).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Area).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => x.OrderId).IsUnique();
        }
    }
}
