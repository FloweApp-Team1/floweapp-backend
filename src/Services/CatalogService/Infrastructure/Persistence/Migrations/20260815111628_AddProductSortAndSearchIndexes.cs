using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSortAndSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_products_createdat_id",
                schema: "Catalog",
                table: "Products",
                columns: new[] { "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "ix_products_price_id",
                schema: "Catalog",
                table: "Products",
                columns: new[] { "Price", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_createdat_id",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "ix_products_price_id",
                schema: "Catalog",
                table: "Products");
        }
    }
}
