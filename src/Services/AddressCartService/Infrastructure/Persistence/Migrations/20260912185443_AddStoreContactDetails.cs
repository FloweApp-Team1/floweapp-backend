using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressCartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreContactDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "AddressCart",
                table: "Stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "AddressCart",
                table: "Stores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                schema: "AddressCart",
                table: "Stores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "AddressCart",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "AddressCart",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                schema: "AddressCart",
                table: "Stores");
        }
    }
}
