using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Auth");

            migrationBuilder.RenameTable(
                name: "VehicleInfos",
                newName: "VehicleInfos",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "UserRoles",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Roles",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefreshTokens",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "OtpCodes",
                newName: "OtpCodes",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "LoginAttempts",
                newName: "LoginAttempts",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "Deliveries",
                newName: "Deliveries",
                newSchema: "Auth");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "Customers",
                newSchema: "Auth");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "VehicleInfos",
                schema: "Auth",
                newName: "VehicleInfos");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "Auth",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "Auth",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "Auth",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                schema: "Auth",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "OtpCodes",
                schema: "Auth",
                newName: "OtpCodes");

            migrationBuilder.RenameTable(
                name: "LoginAttempts",
                schema: "Auth",
                newName: "LoginAttempts");

            migrationBuilder.RenameTable(
                name: "Deliveries",
                schema: "Auth",
                newName: "Deliveries");

            migrationBuilder.RenameTable(
                name: "Customers",
                schema: "Auth",
                newName: "Customers");
        }
    }
}
