using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class refactordbforsprint1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "Auth",
                table: "VehicleInfos");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                schema: "Auth",
                table: "DriverApplications");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Auth",
                table: "DriverApplications",
                newName: "LastName");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleTypeId",
                schema: "Auth",
                table: "VehicleInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                schema: "Auth",
                table: "DriverApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "Auth",
                table: "DriverApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleTypeId",
                schema: "Auth",
                table: "DriverApplications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInfos_VehicleTypeId",
                schema: "Auth",
                table: "VehicleInfos",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverApplications_VehicleTypeId",
                schema: "Auth",
                table: "DriverApplications",
                column: "VehicleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverApplications_VehicleTypes_VehicleTypeId",
                schema: "Auth",
                table: "DriverApplications",
                column: "VehicleTypeId",
                principalSchema: "Auth",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleInfos_VehicleTypes_VehicleTypeId",
                schema: "Auth",
                table: "VehicleInfos",
                column: "VehicleTypeId",
                principalSchema: "Auth",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverApplications_VehicleTypes_VehicleTypeId",
                schema: "Auth",
                table: "DriverApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleInfos_VehicleTypes_VehicleTypeId",
                schema: "Auth",
                table: "VehicleInfos");

            migrationBuilder.DropTable(
                name: "VehicleTypes",
                schema: "Auth");

            migrationBuilder.DropIndex(
                name: "IX_VehicleInfos_VehicleTypeId",
                schema: "Auth",
                table: "VehicleInfos");

            migrationBuilder.DropIndex(
                name: "IX_DriverApplications_VehicleTypeId",
                schema: "Auth",
                table: "DriverApplications");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                schema: "Auth",
                table: "VehicleInfos");

            migrationBuilder.DropColumn(
                name: "FcmToken",
                schema: "Auth",
                table: "DriverApplications");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "Auth",
                table: "DriverApplications");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                schema: "Auth",
                table: "DriverApplications");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "Auth",
                table: "DriverApplications",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "Auth",
                table: "VehicleInfos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                schema: "Auth",
                table: "DriverApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
