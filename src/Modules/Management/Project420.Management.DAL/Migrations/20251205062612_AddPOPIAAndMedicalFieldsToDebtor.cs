using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project420.Management.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPOPIAAndMedicalFieldsToDebtor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentDate",
                table: "Debtors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentGiven",
                table: "Debtors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConsentPurpose",
                table: "Debtors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarketingConsent",
                table: "Debtors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MedicalPermitExpiryDate",
                table: "Debtors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalPermitNumber",
                table: "Debtors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Debtors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTerms",
                table: "Debtors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PostalAddress",
                table: "Debtors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescribingDoctor",
                table: "Debtors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentDate",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "ConsentGiven",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "ConsentPurpose",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "MarketingConsent",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "MedicalPermitExpiryDate",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "MedicalPermitNumber",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "PaymentTerms",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "PostalAddress",
                table: "Debtors");

            migrationBuilder.DropColumn(
                name: "PrescribingDoctor",
                table: "Debtors");
        }
    }
}
