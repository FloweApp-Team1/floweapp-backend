using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configrations
{
    public class RefreshTokenConfigration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Token).IsRequired();
            builder.Property(x => x.ExpiresAt).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.RevokedAt).IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
