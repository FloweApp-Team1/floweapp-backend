using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IconUrl)
                .HasMaxLength(2048);

            // Default keeps rows created before this column existed visible in the bar.
            builder.Property(c => c.ActiveStatus)
                .HasConversion<string>()
                .HasDefaultValue(ActiveStatus.Active);

            builder.HasIndex(x => x.Name);

            // Serves GET /categories: active only, ordered by DisplayOrder.
            builder.HasIndex(x => new { x.ActiveStatus, x.DisplayOrder });
        }
    }
}
