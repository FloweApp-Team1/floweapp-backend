using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configrations
{
    public class VehicleInfoConfiguration : IEntityTypeConfiguration<VehicleInfo>
    {
        public void Configure(EntityTypeBuilder<VehicleInfo> builder)
        {
            builder.ToTable("VehicleInfos");
            builder.Property(e => e.PlateNumber).IsRequired();
            builder.Property(e => e.Capacity).IsRequired();
            builder.Property(e => e.Type).IsRequired();

            builder.HasOne(e => e.Delivery)
                .WithOne(d => d.VehicleInfo)
                .HasForeignKey<VehicleInfo>(e => e.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
