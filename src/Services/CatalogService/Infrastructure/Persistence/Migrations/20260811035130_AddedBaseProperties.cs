using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedBaseProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Catalog",
                table: "ProductImages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "ProductImages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "ProductImages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "Occasions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Occasions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Catalog",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "Occasions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Occasions");

            migrationBuilder.DropColumn(
                name: "LastChangedBy",
                schema: "Catalog",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Categories");
        }
    }
}
