using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using System.Reflection;

namespace PaymentService.Infrastructure.Persistence
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentAttempt> PaymentAttempts { get; set; } = null!;
        public DbSet<WebhookEvent> WebhookEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Payment");
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
