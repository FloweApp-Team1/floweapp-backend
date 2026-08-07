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
            builder.Property(x => x.PhoneNumber).IsRequired();
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.Property(x => x.BirthDate).IsRequired();
            builder.Property(x => x.Gender).IsRequired();

            // ForgetPassword/VerifyOtp/ResetPassword/Login all look users up by email;
            // not unique because a deleted account's row (IsDeleted) can share an email
            // with the account that replaces it.
            builder.HasIndex(x => x.Email);
        }
    }
}
