using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace OrdersService.Infrastructure.Persistence
{
    public class OrdersDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderAddressSnapshot> OrderAddressSnapshots { get; set; }
        public DbSet<DriverLocation> DriverLocations { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
        {
        }

        // Applied context-wide rather than per property so no timestamp added later can
        // silently go back to being kind-less. See UtcDateTimeConverter.
        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            base.ConfigureConventions(builder);

            builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
            builder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("Orders");

            builder.Entity<Order>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<OrderItem>().HasQueryFilter(x => !x.Order.IsDeleted);
            builder.Entity<OrderAddressSnapshot>().HasQueryFilter(x => !x.Order.IsDeleted);
            builder.Entity<DriverLocation>().HasQueryFilter(x => !x.Order.IsDeleted);
            builder.Entity<OrderStatusHistory>().HasQueryFilter(x => !x.Order.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);

            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            builder.AddOutboxStateEntity();
        }
    }
}
