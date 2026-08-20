using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations
{
    public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
    {
        public void Configure(EntityTypeBuilder<WebhookEvent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StripeEventId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(255);
            
            // Payload could be large JSON
            builder.Property(x => x.Payload)
                .IsRequired();

            // Index to ensure we do not process the same Stripe Event multiple times
            builder.HasIndex(x => x.StripeEventId)
                .IsUnique();
        }
    }
}
