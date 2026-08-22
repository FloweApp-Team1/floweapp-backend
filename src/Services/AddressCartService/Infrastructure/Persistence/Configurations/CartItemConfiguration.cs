using AddressCartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AddressCartService.Infrastructure.Persistence.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity).IsRequired();

            builder.Property(x => x.PriceAtAdd)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            // One line per product per cart - AddCartItem increments quantity instead of
            // inserting a duplicate line for a product already in the cart.
            builder.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
        }
    }
}
