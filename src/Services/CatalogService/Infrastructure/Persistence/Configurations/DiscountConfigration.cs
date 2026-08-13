using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class DiscountConfigration: IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Percentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(d => d.StartDate)
                .IsRequired();

            builder.Property(d => d.EndDate)
                .IsRequired();

            builder.HasOne(d => d.Product)
                .WithMany(p => p.Discounts)
                .HasForeignKey(d => d.ProductId);
        }
    }
}