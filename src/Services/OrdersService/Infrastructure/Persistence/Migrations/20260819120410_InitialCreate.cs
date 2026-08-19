using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdersService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Orders");

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DriverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DriverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DriverAssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsGift = table.Column<bool>(type: "bit", nullable: false),
                    GiftRecipientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    GiftRecipientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GiftRecipientAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverLocations",
                schema: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Lng = table.Column<double>(type: "float", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastBroadcastAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverLocations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Orders",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAddressSnapshots",
                schema: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RecipientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Lat = table.Column<double>(type: "float", nullable: true),
                    Lng = table.Column<double>(type: "float", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAddressSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAddressSnapshots_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Orders",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                schema: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProductImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Orders",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocations_DriverId",
                schema: "Orders",
                table: "DriverLocations",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocations_OrderId",
                schema: "Orders",
                table: "DriverLocations",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocations_RecordedAt",
                schema: "Orders",
                table: "DriverLocations",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddressSnapshots_OrderId",
                schema: "Orders",
                table: "OrderAddressSnapshots",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "Orders",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DriverId",
                schema: "Orders",
                table: "Orders",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                schema: "Orders",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                schema: "Orders",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId",
                schema: "Orders",
                table: "Orders",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_CreatedAt",
                schema: "Orders",
                table: "Orders",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverLocations",
                schema: "Orders");

            migrationBuilder.DropTable(
                name: "OrderAddressSnapshots",
                schema: "Orders");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "Orders");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "Orders");
        }
    }
}
