using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductOccasions_Occasions_OccasionsId",
                schema: "Catalog",
                table: "ProductOccasions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductOccasions_Products_ProductsId",
                schema: "Catalog",
                table: "ProductOccasions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductOccasions",
                schema: "Catalog",
                table: "ProductOccasions");

            migrationBuilder.RenameTable(
                name: "ProductOccasions",
                schema: "Catalog",
                newName: "OccasionProduct",
                newSchema: "Catalog");

            migrationBuilder.RenameIndex(
                name: "IX_ProductOccasions_ProductsId",
                schema: "Catalog",
                table: "OccasionProduct",
                newName: "IX_OccasionProduct_ProductsId");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                schema: "Catalog",
                table: "Products",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OccasionProduct",
                schema: "Catalog",
                table: "OccasionProduct",
                columns: new[] { "OccasionsId", "ProductsId" });

            migrationBuilder.CreateTable(
                name: "ProductInclude",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInclude", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductInclude_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductOccasion",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccasionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOccasion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOccasion_Occasions_OccasionId",
                        column: x => x.OccasionId,
                        principalSchema: "Catalog",
                        principalTable: "Occasions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductOccasion_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductInclude_ProductId",
                schema: "Catalog",
                table: "ProductInclude",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOccasion_OccasionId",
                schema: "Catalog",
                table: "ProductOccasion",
                column: "OccasionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOccasion_ProductId",
                schema: "Catalog",
                table: "ProductOccasion",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OccasionProduct_Occasions_OccasionsId",
                schema: "Catalog",
                table: "OccasionProduct",
                column: "OccasionsId",
                principalSchema: "Catalog",
                principalTable: "Occasions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OccasionProduct_Products_ProductsId",
                schema: "Catalog",
                table: "OccasionProduct",
                column: "ProductsId",
                principalSchema: "Catalog",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OccasionProduct_Occasions_OccasionsId",
                schema: "Catalog",
                table: "OccasionProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_OccasionProduct_Products_ProductsId",
                schema: "Catalog",
                table: "OccasionProduct");

            migrationBuilder.DropTable(
                name: "ProductInclude",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "ProductOccasion",
                schema: "Catalog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OccasionProduct",
                schema: "Catalog",
                table: "OccasionProduct");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "OccasionProduct",
                schema: "Catalog",
                newName: "ProductOccasions",
                newSchema: "Catalog");

            migrationBuilder.RenameIndex(
                name: "IX_OccasionProduct_ProductsId",
                schema: "Catalog",
                table: "ProductOccasions",
                newName: "IX_ProductOccasions_ProductsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductOccasions",
                schema: "Catalog",
                table: "ProductOccasions",
                columns: new[] { "OccasionsId", "ProductsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOccasions_Occasions_OccasionsId",
                schema: "Catalog",
                table: "ProductOccasions",
                column: "OccasionsId",
                principalSchema: "Catalog",
                principalTable: "Occasions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOccasions_Products_ProductsId",
                schema: "Catalog",
                table: "ProductOccasions",
                column: "ProductsId",
                principalSchema: "Catalog",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
