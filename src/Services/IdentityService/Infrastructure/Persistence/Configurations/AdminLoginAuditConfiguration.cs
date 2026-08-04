using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configrations
{
    public class AdminLoginAuditConfiguration : IEntityTypeConfiguration<AdminLoginAudit>
    {
        public void Configure(EntityTypeBuilder<AdminLoginAudit> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
            builder.Property(e => e.IpAddress).HasMaxLength(45).IsRequired(); 
            builder.Property(e => e.UserAgent).HasMaxLength(500);
            builder.Property(x => x.IsSuccess).IsRequired();

        }
    }
}
