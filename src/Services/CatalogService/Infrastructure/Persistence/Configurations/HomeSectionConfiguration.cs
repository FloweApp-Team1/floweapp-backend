using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class HomeSectionConfiguration : IEntityTypeConfiguration<HomeSection>
    {
        public void Configure(EntityTypeBuilder<HomeSection> builder)
        {
            builder.ToTable("HomeSections");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
            builder.Property(x => x.ViewAllDeeplink).HasMaxLength(2048);
            builder.Property(x => x.BannerImageUrl).HasMaxLength(2048);
            builder.Property(x => x.BannerDeeplink).HasMaxLength(2048);
            builder.HasIndex(x => new { x.Enabled, x.Order });

            builder.HasMany(x => x.SectionCategories)
                .WithOne(x => x.HomeSection)
                .HasForeignKey(x => x.HomeSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.SectionOccasions)
                .WithOne(x => x.HomeSection)
                .HasForeignKey(x => x.HomeSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.SectionProducts)
                .WithOne(x => x.HomeSection)
                .HasForeignKey(x => x.HomeSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class HomeSectionCategoryConfiguration : IEntityTypeConfiguration<HomeSectionCategory>
    {
        public void Configure(EntityTypeBuilder<HomeSectionCategory> builder)
        {
            builder.ToTable("HomeSectionCategories");
            builder.HasKey(x => new { x.HomeSectionId, x.CategoryId });
            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class HomeSectionOccasionConfiguration : IEntityTypeConfiguration<HomeSectionOccasion>
    {
        public void Configure(EntityTypeBuilder<HomeSectionOccasion> builder)
        {
            builder.ToTable("HomeSectionOccasions");
            builder.HasKey(x => new { x.HomeSectionId, x.OccasionId });
            builder.HasOne(x => x.Occasion)
                .WithMany()
                .HasForeignKey(x => x.OccasionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class HomeSectionProductConfiguration : IEntityTypeConfiguration<HomeSectionProduct>
    {
        public void Configure(EntityTypeBuilder<HomeSectionProduct> builder)
        {
            builder.ToTable("HomeSectionProducts");
            builder.HasKey(x => new { x.HomeSectionId, x.ProductId });
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
