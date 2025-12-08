using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project420.Retail.POS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProductBarcodeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductBarcodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BarcodeValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BarcodeType = table.Column<int>(type: "int", nullable: false),
                    IsUnique = table.Column<bool>(type: "bit", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSold = table.Column<bool>(type: "bit", nullable: false),
                    SoldInTransactionId = table.Column<int>(type: "int", nullable: true),
                    SoldDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryBarcode = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBarcodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBarcodes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_BarcodeValue",
                table: "ProductBarcodes",
                column: "BarcodeValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_BatchNumber",
                table: "ProductBarcodes",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_IsSold",
                table: "ProductBarcodes",
                column: "IsSold");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_ProductId",
                table: "ProductBarcodes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_SerialNumber",
                table: "ProductBarcodes",
                column: "SerialNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductBarcodes");
        }
    }
}
