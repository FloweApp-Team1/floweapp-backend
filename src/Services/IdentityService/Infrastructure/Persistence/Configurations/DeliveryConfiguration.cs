using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configrations
{
    public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.ToTable("Deliveries");

            builder.Property(e => e.NationalIdNumber).IsRequired();
            builder.Property(e => e.VehiclePlateNumber).IsRequired();
            builder.Property(e => e.LicenseDocument).IsRequired();
           
        }
    }
}
