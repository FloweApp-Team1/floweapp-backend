using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations
{
    public class UserDeviceTokenConfiguration : IEntityTypeConfiguration<UserDeviceToken>
    {
        public void Configure(EntityTypeBuilder<UserDeviceToken> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DeviceId).IsRequired().HasMaxLength(255);
            builder.Property(x => x.FcmToken).IsRequired();
            
            builder.HasOne(x => x.User)
                   .WithMany(x => x.DeviceTokens)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique();
        }
    }
}
