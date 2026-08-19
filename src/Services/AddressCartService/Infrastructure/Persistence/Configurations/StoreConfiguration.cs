using AddressCartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AddressCartService.Infrastructure.Persistence.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Stores");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.OwnsOne(x => x.Location, location =>
            {
                location.Property(l => l.AddressLine)
                    .HasColumnName("LocationAddressLine")
                    .IsRequired()
                    .HasMaxLength(500);

                location.Property(l => l.Lat).HasColumnName("LocationLat").IsRequired();
                location.Property(l => l.Lng).HasColumnName("LocationLng").IsRequired();
            });

            // Exactly one of Polygon/Radius/CityAreas is populated, matching Type - see
            // CoverageArea's remarks. Polygon/CityAreas stay raw JSON text columns until
            // the polygon-editing flow is built (candidate for EF Core owned-JSON mapping
            // via .ToJson() once the shape stabilizes).
            builder.OwnsOne(x => x.CoverageArea, coverage =>
            {
                coverage.Property(c => c.Type)
                    .HasColumnName("CoverageType")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                coverage.Property(c => c.PolygonJson)
                    .HasColumnName("CoveragePolygonJson")
                    .HasColumnType("nvarchar(max)");

                coverage.Property(c => c.RadiusCenterLat).HasColumnName("CoverageRadiusCenterLat");
                coverage.Property(c => c.RadiusCenterLng).HasColumnName("CoverageRadiusCenterLng");
                coverage.Property(c => c.RadiusKm).HasColumnName("CoverageRadiusKm");

                coverage.Property(c => c.CityAreasJson)
                    .HasColumnName("CoverageCityAreasJson")
                    .HasColumnType("nvarchar(max)");
            });

            builder.Navigation(x => x.Location).IsRequired();
            builder.Navigation(x => x.CoverageArea).IsRequired();

            builder.HasIndex(x => x.Status);
        }
    }
}
