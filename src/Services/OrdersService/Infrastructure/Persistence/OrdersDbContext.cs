using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Infrastructure.Persistence
{
    public class OrdersDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderAddressSnapshot> OrderAddressSnapshots { get; set; }
        public DbSet<DriverLocation> DriverLocations { get; set; }

        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("Orders");

            builder.Entity<Order>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<OrderItem>().HasQueryFilter(x => !x.Order.IsDeleted);
            builder.Entity<OrderAddressSnapshot>().HasQueryFilter(x => !x.Order.IsDeleted);
            builder.Entity<DriverLocation>().HasQueryFilter(x => !x.Order.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        }
    }
}
