using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project420.Production.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HarvestBatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StrainName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartingWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FinalWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WasteWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductionBatchStatus = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    QualityControlPassed = table.Column<bool>(type: "bit", nullable: true),
                    LabTestPassed = table.Column<bool>(type: "bit", nullable: true),
                    THCPercentage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CBDPercentage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UnitsPackaged = table.Column<int>(type: "int", nullable: true),
                    PackageSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PackagingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BatchStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_ProductionBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabTests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionBatchId = table.Column<int>(type: "int", nullable: false),
                    LabName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LabCertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    COANumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SampleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResultsDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    THCPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CBDPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    TotalCannabinoidsPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    PesticidesPassed = table.Column<bool>(type: "bit", nullable: true),
                    HeavyMetalsPassed = table.Column<bool>(type: "bit", nullable: true),
                    MicrobialPassed = table.Column<bool>(type: "bit", nullable: true),
                    OverallPass = table.Column<bool>(type: "bit", nullable: false),
                    FailureDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    COADocumentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_LabTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabTests_ProductionBatches_ProductionBatchId",
                        column: x => x.ProductionBatchId,
                        principalTable: "ProductionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionBatchId = table.Column<int>(type: "int", nullable: false),
                    StepType = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EndWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WasteGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Temperature = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Humidity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ProcessingSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingSteps_ProductionBatches_ProductionBatchId",
                        column: x => x.ProductionBatchId,
                        principalTable: "ProductionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionBatchId = table.Column<int>(type: "int", nullable: false),
                    CheckType = table.Column<int>(type: "int", nullable: false),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Inspector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    Results = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefectsFound = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CorrectiveActions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_QualityControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityControls_ProductionBatches_ProductionBatchId",
                        column: x => x.ProductionBatchId,
                        principalTable: "ProductionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_COANumber",
                table: "LabTests",
                column: "COANumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_OverallPass",
                table: "LabTests",
                column: "OverallPass");

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_ProductionBatchId",
                table: "LabTests",
                column: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingSteps_ProductionBatchId",
                table: "ProcessingSteps",
                column: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingSteps_Status",
                table: "ProcessingSteps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingSteps_StepType",
                table: "ProcessingSteps",
                column: "StepType");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBatches_BatchNumber",
                table: "ProductionBatches",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBatches_HarvestBatchNumber",
                table: "ProductionBatches",
                column: "HarvestBatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBatches_IsActive",
                table: "ProductionBatches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBatches_ProductionBatchStatus",
                table: "ProductionBatches",
                column: "ProductionBatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBatches_StrainName",
                table: "ProductionBatches",
                column: "StrainName");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_CheckType",
                table: "QualityControls",
                column: "CheckType");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_Passed",
                table: "QualityControls",
                column: "Passed");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_ProductionBatchId",
                table: "QualityControls",
                column: "ProductionBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabTests");

            migrationBuilder.DropTable(
                name: "ProcessingSteps");

            migrationBuilder.DropTable(
                name: "QualityControls");

            migrationBuilder.DropTable(
                name: "ProductionBatches");
        }
    }
}
