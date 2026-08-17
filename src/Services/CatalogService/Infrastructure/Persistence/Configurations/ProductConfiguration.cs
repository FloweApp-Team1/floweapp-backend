using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(x => x.StockQuantity)
                .IsRequired();

            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.CategoryId);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.DiscountPercentage)
                .HasColumnType("decimal(5,2)");

            builder.HasMany(x => x.Includes)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ProductOccasions)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
