using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Archiving is now expressed by IsDeleted. Carry the flag across before
            // the column goes, or already-archived rows come back into the bar.
            migrationBuilder.Sql(
                "UPDATE [Catalog].[Categories] SET [IsDeleted] = 1 WHERE [ActiveStatus] = N'Archived';");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ActiveStatus_DisplayOrder",
                schema: "Catalog",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                schema: "Catalog",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsDeleted_DisplayOrder",
                schema: "Catalog",
                table: "Categories",
                columns: new[] { "IsDeleted", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_IsDeleted_DisplayOrder",
                schema: "Catalog",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "ActiveStatus",
                schema: "Catalog",
                table: "Categories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql(
                "UPDATE [Catalog].[Categories] SET [ActiveStatus] = N'Archived' WHERE [IsDeleted] = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ActiveStatus_DisplayOrder",
                schema: "Catalog",
                table: "Categories",
                columns: new[] { "ActiveStatus", "DisplayOrder" });
        }
    }
}
