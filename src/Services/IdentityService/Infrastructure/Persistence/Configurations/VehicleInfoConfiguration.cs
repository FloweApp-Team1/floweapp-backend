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

            builder.HasOne(v => v.VehicleType)
            .WithMany(vt => vt.Vehicles)
            .HasForeignKey(v => v.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Delivery)
                .WithOne(d => d.VehicleInfo)
                .HasForeignKey<VehicleInfo>(e => e.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
