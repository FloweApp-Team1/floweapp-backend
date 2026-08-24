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
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<City> Cities { get; set; }

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

            builder.Entity<Governorate>().HasKey(g => g.Id);
            builder.Entity<Governorate>().Property(g => g.Id).ValueGeneratedNever();
            
            builder.Entity<City>().HasKey(c => c.Id);
            builder.Entity<City>().Property(c => c.Id).ValueGeneratedNever();

            builder.Entity<Governorate>().HasMany(g => g.Cities).WithOne(c => c.Governorate).HasForeignKey(c => c.GovernorateId);



            builder.Entity<Address>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Store>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Cart>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<CartItem>().HasQueryFilter(x => !x.Cart.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(typeof(AddressCartDbContext).Assembly);
        }
    }
}
