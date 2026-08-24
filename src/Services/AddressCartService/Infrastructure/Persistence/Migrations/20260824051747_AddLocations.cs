using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressCartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                schema: "AddressCart",
                table: "Addresses");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                schema: "AddressCart",
                table: "Addresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId",
                schema: "AddressCart",
                table: "Addresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Governorates",
                schema: "AddressCart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                schema: "AddressCart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    GovernorateId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalSchema: "AddressCart",
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CityId",
                schema: "AddressCart",
                table: "Addresses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_GovernorateId",
                schema: "AddressCart",
                table: "Addresses",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_GovernorateId",
                schema: "AddressCart",
                table: "Cities",
                column: "GovernorateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Cities_CityId",
                schema: "AddressCart",
                table: "Addresses",
                column: "CityId",
                principalSchema: "AddressCart",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Governorates_GovernorateId",
                schema: "AddressCart",
                table: "Addresses",
                column: "GovernorateId",
                principalSchema: "AddressCart",
                principalTable: "Governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Cities_CityId",
                schema: "AddressCart",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Governorates_GovernorateId",
                schema: "AddressCart",
                table: "Addresses");

            migrationBuilder.DropTable(
                name: "Cities",
                schema: "AddressCart");

            migrationBuilder.DropTable(
                name: "Governorates",
                schema: "AddressCart");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CityId",
                schema: "AddressCart",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_GovernorateId",
                schema: "AddressCart",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "AddressCart",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "GovernorateId",
                schema: "AddressCart",
                table: "Addresses");

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "AddressCart",
                table: "Addresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
