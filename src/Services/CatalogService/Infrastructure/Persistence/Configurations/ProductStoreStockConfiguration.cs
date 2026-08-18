using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class ProductStoreStockConfiguration : IEntityTypeConfiguration<ProductStoreStock>
    {
        public void Configure(EntityTypeBuilder<ProductStoreStock> builder)
        {
            builder.ToTable("ProductStoreStocks");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Quantity).IsRequired();
            builder.HasIndex(x => new { x.ProductId, x.StoreId }).IsUnique();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.StoreStocks)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
