using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Persistence
{
    public class CatalogDbContext : DbContext
    {
        //public DbSet<Product> Products { get; set; }
        //public DbSet<ProductImage> ProductImages { get; set; }
        //public DbSet<Category> Categories { get; set; }
        //public DbSet<Occasion> Occasions { get; set; }

        //public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        //{
        //}

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    builder.HasDefaultSchema("Catalog");

        //    builder.Entity<Product>().HasQueryFilter(x => !x.IsDeleted);
        //    builder.Entity<ProductImage>().HasQueryFilter(x => !x.Product.IsDeleted);
        //    builder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
        //    builder.Entity<Occasion>().HasQueryFilter(x => !x.IsDeleted);

        //    builder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        //}

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductStoreStock> ProductStoreStocks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Occasion> Occasions { get; set; }
        // Server-driven home layout read model - distinct from the admin-authored
        // HomeSection below; see HomeSectionType vs HomeSectionKind.
        public DbSet<HomeLayoutSection> HomeLayoutSections { get; set; }
        public DbSet<HomeSection> HomeSections { get; set; }
        public DbSet<HomeSectionCategory> HomeSectionCategories { get; set; }
        public DbSet<HomeSectionOccasion> HomeSectionOccasions { get; set; }
        public DbSet<HomeSectionProduct> HomeSectionProducts { get; set; }
        public DbSet<ContentChangeLog> ContentChangeLogs { get; set; }

        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Catalog");

            builder.Entity<Product>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<ProductImage>().HasQueryFilter(x => !x.Product.IsDeleted);
            builder.Entity<ProductStoreStock>().HasQueryFilter(x => !x.Product.IsDeleted);
            builder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Occasion>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<HomeSection>().HasQueryFilter(x => !x.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        }
    }
}
