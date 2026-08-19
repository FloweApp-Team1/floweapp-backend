using AddressCartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AddressCartService.Infrastructure.Persistence.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RecipientName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.RecipientPhone).IsRequired().HasMaxLength(20);
            builder.Property(x => x.AddressLine).IsRequired().HasMaxLength(500);
            builder.Property(x => x.City).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Area).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Label).HasMaxLength(50);

            builder.HasIndex(x => new { x.UserId, x.IsDefault });

            // Local FK: Store is owned by this same service, unlike UserId above.
            builder.HasOne(x => x.Store)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
