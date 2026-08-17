using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations
{
    public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
    {
        public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StripeSessionId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.StripePaymentIntentId)
                .HasMaxLength(255);

            builder.Property(x => x.SessionUrl)
                .HasMaxLength(2000);

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            // Indexes
            builder.HasIndex(x => x.OrderId);

            builder.HasIndex(x => x.StripeSessionId)
                .IsUnique();

            builder.HasIndex(x => x.StripePaymentIntentId)
                .IsUnique()
                .HasFilter("[StripePaymentIntentId] IS NOT NULL");
        }
    }
}
