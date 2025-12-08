using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project420.Retail.POS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameTransactionsToPOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys that reference the tables being renamed
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_TransactionHeaders_TransactionHeaderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_TransactionHeaders_TransactionHeaderId",
                table: "TransactionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionHeaders_TransactionHeaders_OriginalTransactionId",
                table: "TransactionHeaders");

            // Rename tables (preserves data)
            migrationBuilder.RenameTable(
                name: "TransactionHeaders",
                newName: "POSTransactionHeaders");

            migrationBuilder.RenameTable(
                name: "TransactionDetails",
                newName: "POSTransactionDetails");

            // Rename column in POSTransactionDetails
            migrationBuilder.RenameColumn(
                name: "TransactionHeaderId",
                table: "POSTransactionDetails",
                newName: "POSTransactionHeaderId");

            // Rename indexes
            migrationBuilder.RenameIndex(
                name: "IX_TransactionHeaders_Date",
                table: "POSTransactionHeaders",
                newName: "IX_POSTransactionHeaders_Date");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionHeaders_DebtorId",
                table: "POSTransactionHeaders",
                newName: "IX_POSTransactionHeaders_DebtorId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionHeaders_Number",
                table: "POSTransactionHeaders",
                newName: "IX_POSTransactionHeaders_Number");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionHeaders_OriginalTransactionId",
                table: "POSTransactionHeaders",
                newName: "IX_POSTransactionHeaders_OriginalTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionHeaders_PricelistId",
                table: "POSTransactionHeaders",
                newName: "IX_POSTransactionHeaders_PricelistId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionDetails_HeaderId",
                table: "POSTransactionDetails",
                newName: "IX_POSTransactionDetails_HeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionDetails_ProductId",
                table: "POSTransactionDetails",
                newName: "IX_POSTransactionDetails_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionDetails_ProductId1",
                table: "POSTransactionDetails",
                newName: "IX_POSTransactionDetails_ProductId1");

            // Re-create foreign keys with new table/column names
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_POSTransactionHeaders_TransactionHeaderId",
                table: "Payments",
                column: "TransactionHeaderId",
                principalTable: "POSTransactionHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_POSTransactionDetails_POSTransactionHeaders_POSTransactionHeaderId",
                table: "POSTransactionDetails",
                column: "POSTransactionHeaderId",
                principalTable: "POSTransactionHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_POSTransactionHeaders_POSTransactionHeaders_OriginalTransactionId",
                table: "POSTransactionHeaders",
                column: "OriginalTransactionId",
                principalTable: "POSTransactionHeaders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys that reference the renamed tables
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_POSTransactionHeaders_TransactionHeaderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_POSTransactionDetails_POSTransactionHeaders_POSTransactionHeaderId",
                table: "POSTransactionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_POSTransactionHeaders_POSTransactionHeaders_OriginalTransactionId",
                table: "POSTransactionHeaders");

            // Rename tables back to original names (preserves data)
            migrationBuilder.RenameTable(
                name: "POSTransactionHeaders",
                newName: "TransactionHeaders");

            migrationBuilder.RenameTable(
                name: "POSTransactionDetails",
                newName: "TransactionDetails");

            // Rename column back to original name
            migrationBuilder.RenameColumn(
                name: "POSTransactionHeaderId",
                table: "TransactionDetails",
                newName: "TransactionHeaderId");

            // Rename indexes back to original names
            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionHeaders_Date",
                table: "TransactionHeaders",
                newName: "IX_TransactionHeaders_Date");

            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionHeaders_DebtorId",
                table: "TransactionHeaders",
                newName: "IX_TransactionHeaders_DebtorId");

            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionHeaders_Number",
                table: "TransactionHeaders",
                newName: "IX_TransactionHeaders_Number");

            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionHeaders_OriginalTransactionId",
                table: "TransactionHeaders",
                newName: "IX_TransactionHeaders_OriginalTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionHeaders_PricelistId",
                table: "TransactionHeaders",
                newName: "IX_TransactionHeaders_PricelistId");

            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionDetails_HeaderId",
                table: "TransactionDetails",
                newName: "IX_TransactionDetails_HeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionDetails_ProductId",
                table: "TransactionDetails",
                newName: "IX_TransactionDetails_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_POSTransactionDetails_ProductId1",
                table: "TransactionDetails",
                newName: "IX_TransactionDetails_ProductId1");

            // Re-create foreign keys with original table/column names
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_TransactionHeaders_TransactionHeaderId",
                table: "Payments",
                column: "TransactionHeaderId",
                principalTable: "TransactionHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_TransactionHeaders_TransactionHeaderId",
                table: "TransactionDetails",
                column: "TransactionHeaderId",
                principalTable: "TransactionHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionHeaders_TransactionHeaders_OriginalTransactionId",
                table: "TransactionHeaders",
                column: "OriginalTransactionId",
                principalTable: "TransactionHeaders",
                principalColumn: "Id");
        }
    }
}
