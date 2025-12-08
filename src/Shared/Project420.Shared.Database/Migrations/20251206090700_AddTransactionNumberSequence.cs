using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project420.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionNumberSequences",
                columns: table => new
                {
                    SequenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrentSequence = table.Column<long>(type: "bigint", nullable: false),
                    StartingSequence = table.Column<long>(type: "bigint", nullable: false),
                    PaddingLength = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionNumberSequences", x => x.SequenceId);
                });

            migrationBuilder.InsertData(
                table: "TransactionNumberSequences",
                columns: new[] { "SequenceId", "CreatedAt", "CreatedBy", "CurrentSequence", "Description", "IsActive", "LastGeneratedAt", "LastUpdatedAt", "LastUpdatedBy", "PaddingLength", "Prefix", "StartingSequence", "TransactionType" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Sales invoices - formal account invoices", true, null, null, null, 5, "INV", 1L, 4 },
                    { 2, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Credit notes - refunds and returns", true, null, null, null, 5, "CRN", 1L, 5 },
                    { 3, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Point of Sale transactions - retail sales", true, null, null, null, 5, "", 1L, 1 },
                    { 4, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Goods received notes - stock received from suppliers", true, null, null, null, 5, "GRV", 1L, 2 },
                    { 5, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Returns to supplier - defective/unwanted stock", true, null, null, null, 5, "RTS", 1L, 3 },
                    { 6, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Stock adjustments - internal corrections", true, null, null, null, 5, "ADJ", 1L, 6 },
                    { 7, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Customer payments - cash/card/EFT received", true, null, null, null, 5, "PAY", 1L, 7 },
                    { 8, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Sales quotations - non-binding estimates", true, null, null, null, 5, "QTE", 1L, 8 },
                    { 9, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Layby transactions - deposit for future collection", true, null, null, null, 5, "LAY", 1L, 9 },
                    { 10, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Stock transfers - inter-location movements", true, null, null, null, 5, "TRF", 1L, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionNumberSequences_IsActive",
                table: "TransactionNumberSequences",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionNumberSequences_LastGeneratedAt",
                table: "TransactionNumberSequences",
                column: "LastGeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionNumberSequences_TransactionType_Unique",
                table: "TransactionNumberSequences",
                column: "TransactionType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionNumberSequences");
        }
    }
}
