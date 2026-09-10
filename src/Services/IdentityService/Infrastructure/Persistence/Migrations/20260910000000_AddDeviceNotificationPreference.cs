using IdentityService.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AuthDbContext))]
    [Migration("20260910000000_AddDeviceNotificationPreference")]
    public partial class AddDeviceNotificationPreference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationsEnabled",
                schema: "Auth",
                table: "UserDeviceTokens",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Preserve the old user-level preference for every already registered device.
            // From this migration onward, the device row is the source of truth.
            migrationBuilder.Sql(
                """
                UPDATE deviceToken
                SET deviceToken.NotificationsEnabled =
                    CASE WHEN [user].NotificationStatus = 0 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
                FROM [Auth].[UserDeviceTokens] AS deviceToken
                INNER JOIN [Auth].[Users] AS [user] ON [user].Id = deviceToken.UserId;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationsEnabled",
                schema: "Auth",
                table: "UserDeviceTokens");
        }
    }
}
