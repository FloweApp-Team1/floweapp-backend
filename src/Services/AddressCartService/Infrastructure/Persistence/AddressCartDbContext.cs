using AddressCartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AddressCartService.Infrastructure.Persistence
{
    public class AddressCartDbContext : DbContext
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public AddressCartDbContext(DbContextOptions<AddressCartDbContext> options) : base(options)
        {
        }

        // Protected non-generic overload allows derived test contexts to pass DbContextOptions<TDerived>,
        // which forces EF Core to use the derived type as the model cache key and call the
        // derived OnModelCreating instead of reusing the cached base-class model.
        protected AddressCartDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("AddressCart");

            builder.Entity<Address>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Store>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Cart>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<CartItem>().HasQueryFilter(x => !x.Cart.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(typeof(AddressCartDbContext).Assembly);
        }
    }
}
