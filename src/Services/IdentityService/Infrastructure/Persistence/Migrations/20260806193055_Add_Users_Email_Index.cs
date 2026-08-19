using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Users_Email_Index : Migration
    {
        // Intentionally empty.
        //
        // This migration was generated on a branch that did not yet contain
        // 20260805093658_AddIndexToEmailAndPhoneNumber, which already creates a UNIQUE
        // IX_Users_Email. After the merge both ran in sequence and the second one failed
        // with "an index or statistics with name 'IX_Users_Email' already exists", so no
        // database could be created from scratch.
        //
        // The unique index from the earlier migration is the one the model asks for, so
        // the duplicate is dropped here instead of being reordered - removing the file
        // outright would leave an orphan row in __EFMigrationsHistory on databases that
        // already recorded it.

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
