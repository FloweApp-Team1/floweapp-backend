using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressCartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDefaultAddressIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId_IsDefault_Unique",
                schema: "AddressCart",
                table: "Addresses",
                column: "UserId",
                unique: true,
                filter: "[IsDefault] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_UserId_IsDefault_Unique",
                schema: "AddressCart",
                table: "Addresses");
        }
    }
}
