using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        //public void Configure(EntityTypeBuilder<Product> builder)
        //{
        //    builder.ToTable("Products");

        //    builder.HasKey(x => x.Id);

        //    builder.Property(x => x.Name)
        //        .IsRequired()
        //        .HasMaxLength(150);

        //    builder.Property(x => x.Description)
        //        .IsRequired()
        //        .HasMaxLength(2000);

        //    builder.Property(x => x.Price)
        //        .HasColumnType("decimal(10,2)")
        //        .IsRequired();

        //    builder.Property(x => x.StockQuantity)
        //        .IsRequired();

        //    builder.HasIndex(x => x.Name);
        //    builder.HasIndex(x => x.CategoryId);

        //    builder.HasOne(x => x.Category)
        //        .WithMany(x => x.Products)
        //        .HasForeignKey(x => x.CategoryId)
        //        .OnDelete(DeleteBehavior.Restrict);
        //}
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
            builder.Property(x => x.DiscountPercent);
            builder.Property(x => x.StockQuantity)
                .IsRequired();

            // Small product-owned string list, stored as JSON rather than a join table.
            var includesComparer = new ValueComparer<List<string>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                v => v.ToList());

            builder.Property(x => x.Includes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions?)null),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
                .Metadata.SetValueComparer(includesComparer);

            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.CategoryId);
            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
