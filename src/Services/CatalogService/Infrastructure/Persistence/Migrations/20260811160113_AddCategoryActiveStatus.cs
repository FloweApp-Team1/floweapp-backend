using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveStatus",
                schema: "Catalog",
                table: "Categories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ActiveStatus_DisplayOrder",
                schema: "Catalog",
                table: "Categories",
                columns: new[] { "ActiveStatus", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ActiveStatus_DisplayOrder",
                schema: "Catalog",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                schema: "Catalog",
                table: "Categories");
        }
    }
}
