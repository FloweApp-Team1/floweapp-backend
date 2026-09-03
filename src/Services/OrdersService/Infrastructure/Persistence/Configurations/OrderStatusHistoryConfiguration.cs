using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistories");

            builder.HasKey(x => x.Id);

            // Stored as its name for the same reason Order.Status is: a reordered enum must
            // not silently repoint historical rows at a different stage. 40 matches
            // OrderConfiguration - the longest name (AwaitingDeliveryConfirmation) is 28.
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();

            builder.Property(x => x.OccurredAt).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            // Serves both readers: the timeline pulls one order's stages in the order they
            // happened, and the writer checks whether a status was already recorded.
            builder.HasIndex(x => new { x.OrderId, x.OccurredAt });

            builder.HasOne(x => x.Order)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
