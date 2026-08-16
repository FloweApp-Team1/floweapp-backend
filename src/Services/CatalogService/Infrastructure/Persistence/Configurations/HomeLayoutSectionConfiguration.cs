using System.Text.Json;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class HomeLayoutSectionConfiguration : IEntityTypeConfiguration<HomeLayoutSection>
    {
        public void Configure(EntityTypeBuilder<HomeLayoutSection> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200);

            builder.Property(x => x.Type)
                .IsRequired();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            builder.Property(x => x.Payload)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<BaseSectionPayload>(v, jsonOptions)!
                )
                .HasColumnType("nvarchar(max)");
        }
    }
}
