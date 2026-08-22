using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class removeduplicatedpropandadduserprop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehiclePlateNumber",
                schema: "Auth",
                table: "Deliveries");

            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                schema: "Auth",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NotifcationStatus",
                schema: "Auth",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                schema: "Auth",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                schema: "Auth",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                schema: "Auth",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                schema: "Auth",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "Auth",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacedByTokenId",
                schema: "Auth",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FcmToken",
                schema: "Auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NotifcationStatus",
                schema: "Auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                schema: "Auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                schema: "Auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                schema: "Auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                schema: "Auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "Auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByTokenId",
                schema: "Auth",
                table: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "VehiclePlateNumber",
                schema: "Auth",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
