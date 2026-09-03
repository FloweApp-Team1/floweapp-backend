using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(30);
            builder.HasIndex(x => x.OrderNumber).IsUnique();

            // 40, not 20: the status is stored as its enum name and the longest
            // (AwaitingDeliveryConfirmation) is 28 characters. See AddAwaitingDeliveryConfirmationStatus.
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(10).IsRequired();
            builder.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(10).IsRequired();

            builder.Property(x => x.Subtotal).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(x => x.DeliveryFee).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(x => x.Total).HasColumnType("decimal(10,2)").IsRequired();

            builder.Property(x => x.DriverName).HasMaxLength(150);
            builder.Property(x => x.DriverPhone).HasMaxLength(20);
            builder.Property(x => x.DriverImageUrl).HasMaxLength(500);

            builder.Property(x => x.GiftRecipientName).HasMaxLength(150);
            builder.Property(x => x.GiftRecipientPhone).HasMaxLength(20);
            builder.Property(x => x.GiftRecipientAddress).HasMaxLength(500);

            builder.HasIndex(x => new { x.UserId, x.CreatedAt });
            builder.HasIndex(x => x.StoreId);
            builder.HasIndex(x => x.DriverId);
            builder.HasIndex(x => x.Status);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AddressSnapshot)
                .WithOne(x => x.Order)
                .HasForeignKey<OrderAddressSnapshot>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
