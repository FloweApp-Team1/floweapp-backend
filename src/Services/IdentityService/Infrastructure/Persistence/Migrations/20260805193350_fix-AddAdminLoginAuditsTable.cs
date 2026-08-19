using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixAddAdminLoginAuditsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminLoginAudits",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLoginAudits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminLoginAudits_Email_AttemptedAt",
                schema: "Auth",
                table: "AdminLoginAudits",
                columns: new[] { "Email", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminLoginAudits_IpAddress_AttemptedAt",
                schema: "Auth",
                table: "AdminLoginAudits",
                columns: new[] { "IpAddress", "AttemptedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminLoginAudits",
                schema: "Auth");
        }
    }
}
