using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdersService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPickupContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreImageUrl",
                schema: "Orders",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorePhoneNumber",
                schema: "Orders",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreWhatsAppNumber",
                schema: "Orders",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreImageUrl",
                schema: "Orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StorePhoneNumber",
                schema: "Orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StoreWhatsAppNumber",
                schema: "Orders",
                table: "Orders");
        }
    }
}
