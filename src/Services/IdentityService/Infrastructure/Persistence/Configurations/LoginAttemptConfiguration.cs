using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configrations
{
    public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.ToTable("LoginAttempts");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.AttemptedAt)
                .IsRequired();

            builder.Property(x => x.IsSuccess)
                .IsRequired();

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45)
                .IsRequired();
        }
    }
}
