using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelSnapshot : Migration
    {
        // Snapshot-only migration: it deliberately emits no SQL.
        //
        // AuthDbContextModelSnapshot had drifted from the entity model across three
        // merged branches - it still declared Users.BirthDate (dropped by
        // 20260805212159_remove-BD, and no longer a property on User) and did not know
        // about Guests (created by 20260808150936_AddGuestEntity). That drift made
        // `dotnet ef migrations add` scaffold changes that were already applied.
        //
        // Both operations EF scaffolded here are already true in any database built from
        // the migration chain, so running them would fail. The value of this migration is
        // the corrected snapshot in its .Designer.cs, which future migrations diff
        // against.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
