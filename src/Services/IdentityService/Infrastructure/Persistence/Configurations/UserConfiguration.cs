using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configrations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
            builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.HasIndex(x => x.Email)
             .IsUnique();
            builder.HasIndex(x => x.PhoneNumber)
                .IsUnique();
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.Property(x => x.Gender).IsRequired();

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.PhoneNumber).IsUnique();



        }
    }
}
