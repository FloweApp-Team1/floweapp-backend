using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configrations
{
    public class OtpConfiguration : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(EntityTypeBuilder<OtpCode> builder)
        {
            builder.ToTable("OtpCodes");

            builder.HasKey(x => x.Id);
            builder.Property(e => e.Code).HasMaxLength(6).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.ExpiresAt).IsRequired();
            builder.Property(e => e.IsUsed).IsRequired();
        }
    }
}
