using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductInventoryAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "User"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReorderLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReorderPoint = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventory_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "Electronics", "Electronic devices and accessories", true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 2, "Clothing", "Apparel and fashion items", true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 3, "Books", "Books and educational materials", true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 4, "Home & Garden", "Home improvement and garden supplies", true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Username", "Email", "FirstName", "LastName", "PasswordHash", "Role", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "admin", "admin@productinventory.com", "System", "Administrator", "$2a$11$mKGjfSNlWWWFjQgKwjwmjOv8bYDJY1Q8ZxnzKfPxHJxC8rQ7gWX3u", "Admin", true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 2, "manager", "manager@productinventory.com", "Inventory", "Manager", "$2a$11$mKGjfSNlWWWFjQgKwjwmjOv8bYDJY1Q8ZxnzKfPxHJxC8rQ7gWX3u", "Manager", true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Description", "SKU", "CategoryId", "Price", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "Smartphone", "Latest model smartphone", "ELEC-SP-001", 1, 599.99m, true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 2, "Laptop", "High-performance laptop", "ELEC-LP-001", 1, 1299.99m, true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 3, "T-Shirt", "Cotton t-shirt", "CLTH-TS-001", 2, 24.99m, true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 4, "Programming Guide", "Comprehensive programming guide", "BOOK-PG-001", 3, 49.99m, true, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "ProductId", "QuantityOnHand", "Quantity", "ReorderLevel", "ReorderPoint", "MaxStockLevel", "LastUpdated" },
                values: new object[,]
                {
                    { 1, 1, 50, 50, 10, 10, 100, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, 25, 25, 5, 5, 50, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 3, 3, 200, 200, 50, 50, 500, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) },
                    { 4, 4, 75, 75, 15, 15, 150, new DateTime(2025, 10, 18, 2, 19, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventory",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_QuantityOnHand",
                table: "Inventory",
                column: "QuantityOnHand");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_CategoryId",
                table: "Products",
                columns: new[] { "Name", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}