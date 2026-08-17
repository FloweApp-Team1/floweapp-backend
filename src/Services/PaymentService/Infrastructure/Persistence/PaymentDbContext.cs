using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Persistence
{
    public class PaymentDbContext : DbContext
    {

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("Payment");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);

        }
    }
}
