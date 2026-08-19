using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.ProductImageUrl).HasMaxLength(2048);
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(x => x.Quantity).IsRequired();

            builder.HasIndex(x => x.OrderId);
        }
    }
}
