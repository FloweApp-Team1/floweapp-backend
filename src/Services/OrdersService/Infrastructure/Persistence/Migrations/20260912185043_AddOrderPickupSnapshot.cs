using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdersService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPickupSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreAddressLine",
                schema: "Orders",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StoreLat",
                schema: "Orders",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StoreLng",
                schema: "Orders",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreName",
                schema: "Orders",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreAddressLine",
                schema: "Orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StoreLat",
                schema: "Orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StoreLng",
                schema: "Orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StoreName",
                schema: "Orders",
                table: "Orders");
        }
    }
}
