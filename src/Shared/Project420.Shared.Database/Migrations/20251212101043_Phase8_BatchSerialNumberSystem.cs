using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project420.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class Phase8_BatchSerialNumberSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchNumberSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    BatchType = table.Column<int>(type: "int", nullable: false),
                    BatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentSequence = table.Column<int>(type: "int", nullable: false),
                    MaxSequence = table.Column<int>(type: "int", nullable: false),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastGeneratedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_BatchNumberSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullSerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShortSerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    StrainCode = table.Column<int>(type: "int", nullable: false),
                    BatchType = table.Column<int>(type: "int", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchSequence = table.Column<int>(type: "int", nullable: false),
                    UnitSequence = table.Column<int>(type: "int", nullable: false),
                    WeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PackQty = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ProductSKU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentLocationId = table.Column<int>(type: "int", nullable: true),
                    StatusChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoldTransactionId = table.Column<int>(type: "int", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DestructionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestructionWitness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SerialNumbers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialNumberSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    SequenceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchType = table.Column<int>(type: "int", nullable: true),
                    BatchSequence = table.Column<int>(type: "int", nullable: true),
                    CurrentSequence = table.Column<int>(type: "int", nullable: false),
                    MaxSequence = table.Column<int>(type: "int", nullable: false),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastGeneratedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SerialNumberSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchNumberSequences_BatchDate",
                table: "BatchNumberSequences",
                column: "BatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_BatchNumberSequences_SiteId",
                table: "BatchNumberSequences",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchNumberSequences_SiteId_BatchType_BatchDate_Unique",
                table: "BatchNumberSequences",
                columns: new[] { "SiteId", "BatchType", "BatchDate" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_BatchNumber",
                table: "SerialNumbers",
                column: "BatchNumber",
                filter: "[BatchNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_FullSerialNumber_Unique",
                table: "SerialNumbers",
                column: "FullSerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_ProductId",
                table: "SerialNumbers",
                column: "ProductId",
                filter: "[ProductId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_ShortSerialNumber_Unique",
                table: "SerialNumbers",
                column: "ShortSerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_SiteId",
                table: "SerialNumbers",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_SiteId_ProductionDate_Status",
                table: "SerialNumbers",
                columns: new[] { "SiteId", "ProductionDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_Status",
                table: "SerialNumbers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_StrainCode",
                table: "SerialNumbers",
                column: "StrainCode");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumberSequences_Composite",
                table: "SerialNumberSequences",
                columns: new[] { "SiteId", "SequenceType", "ProductionDate", "BatchType", "BatchSequence" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumberSequences_ProductionDate",
                table: "SerialNumberSequences",
                column: "ProductionDate");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumberSequences_SiteId",
                table: "SerialNumberSequences",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchNumberSequences");

            migrationBuilder.DropTable(
                name: "SerialNumbers");

            migrationBuilder.DropTable(
                name: "SerialNumberSequences");
        }
    }
}
