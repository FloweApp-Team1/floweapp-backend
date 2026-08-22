using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class OccasionConfiguration : IEntityTypeConfiguration<Occasion>
    {
        public void Configure(EntityTypeBuilder<Occasion> builder)
        {
            builder.ToTable("Occasions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(2048);

            builder.HasIndex(x => x.Name);

            // we don't need to configure the many-to-many relationship here because we have a join entity (ProductOccasion) that represents the relationship between Product and Occasion. The join entity will be configured in its own configuration class (ProductOccasionConfiguration).

            //builder.HasMany(x => x.Products)
            //    .WithMany(x => x.Occasions)
            //    .UsingEntity(j => j.ToTable("ProductOccasions"));

            builder.HasMany(x => x.ProductOccasions)
            .WithOne(x => x.Occasion)
            .HasForeignKey(x => x.OccasionId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
