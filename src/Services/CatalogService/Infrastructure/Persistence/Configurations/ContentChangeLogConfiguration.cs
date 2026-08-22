using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class ContentChangeLogConfiguration : IEntityTypeConfiguration<ContentChangeLog>
    {
        public void Configure(EntityTypeBuilder<ContentChangeLog> builder)
        {
            builder.ToTable("ContentChangeLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Action).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Summary).HasMaxLength(1000);
            builder.HasIndex(x => new { x.EntityType, x.EntityId, x.ChangedAt });
        }
    }
}
