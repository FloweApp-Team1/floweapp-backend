using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdersService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableOrdersCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_DriverId_CreatedAt",
                schema: "Orders",
                table: "Orders",
                columns: new[] { "Status", "DriverId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_DriverId_CreatedAt",
                schema: "Orders",
                table: "Orders");
        }
    }
}
