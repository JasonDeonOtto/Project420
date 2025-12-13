using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project420.Cultivation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrowRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoomType = table.Column<int>(type: "int", nullable: false),
                    RoomSizeSquareMeters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TargetTemperature = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TargetHumidity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LightCycle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_GrowRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrowCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CycleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StrainName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedHarvestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualHarvestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GrowRoomId = table.Column<int>(type: "int", nullable: true),
                    TotalPlantsStarted = table.Column<int>(type: "int", nullable: false),
                    PlantsHarvested = table.Column<int>(type: "int", nullable: false),
                    TotalWetWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalDryWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_GrowCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowCycles_GrowRooms_GrowRoomId",
                        column: x => x.GrowRoomId,
                        principalTable: "GrowRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HarvestBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GrowCycleId = table.Column<int>(type: "int", nullable: false),
                    StrainName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HarvestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalWetWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDryWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PlantCount = table.Column<int>(type: "int", nullable: false),
                    THCPercentage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CBDPercentage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LabTestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LabTestCertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LabTestPassed = table.Column<bool>(type: "bit", nullable: true),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StorageLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_HarvestBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HarvestBatches_GrowCycles_GrowCycleId",
                        column: x => x.GrowCycleId,
                        principalTable: "GrowCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GrowCycleId = table.Column<int>(type: "int", nullable: false),
                    StrainName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    StageStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HarvestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentGrowRoomId = table.Column<int>(type: "int", nullable: true),
                    PlantSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MotherPlantId = table.Column<int>(type: "int", nullable: true),
                    PlantSex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WetWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DryWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HarvestBatchId = table.Column<int>(type: "int", nullable: true),
                    HealthStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DestroyedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DestructionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WasteWeightGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Plants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plants_GrowCycles_GrowCycleId",
                        column: x => x.GrowCycleId,
                        principalTable: "GrowCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plants_GrowRooms_CurrentGrowRoomId",
                        column: x => x.CurrentGrowRoomId,
                        principalTable: "GrowRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plants_HarvestBatches_HarvestBatchId",
                        column: x => x.HarvestBatchId,
                        principalTable: "HarvestBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plants_Plants_MotherPlantId",
                        column: x => x.MotherPlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrowCycles_CycleCode",
                table: "GrowCycles",
                column: "CycleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrowCycles_GrowRoomId",
                table: "GrowCycles",
                column: "GrowRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowCycles_IsActive",
                table: "GrowCycles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GrowCycles_StartDate",
                table: "GrowCycles",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_GrowCycles_StrainName",
                table: "GrowCycles",
                column: "StrainName");

            migrationBuilder.CreateIndex(
                name: "IX_GrowRooms_IsActive",
                table: "GrowRooms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GrowRooms_RoomCode",
                table: "GrowRooms",
                column: "RoomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrowRooms_RoomType",
                table: "GrowRooms",
                column: "RoomType");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestBatches_BatchNumber",
                table: "HarvestBatches",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HarvestBatches_GrowCycleId",
                table: "HarvestBatches",
                column: "GrowCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestBatches_HarvestDate",
                table: "HarvestBatches",
                column: "HarvestDate");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestBatches_IsActive",
                table: "HarvestBatches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestBatches_StrainName",
                table: "HarvestBatches",
                column: "StrainName");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_CurrentGrowRoomId",
                table: "Plants",
                column: "CurrentGrowRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_CurrentStage",
                table: "Plants",
                column: "CurrentStage");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_GrowCycleId",
                table: "Plants",
                column: "GrowCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_HarvestBatchId",
                table: "Plants",
                column: "HarvestBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_IsActive",
                table: "Plants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_MotherPlantId",
                table: "Plants",
                column: "MotherPlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_PlantTag",
                table: "Plants",
                column: "PlantTag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropTable(
                name: "HarvestBatches");

            migrationBuilder.DropTable(
                name: "GrowCycles");

            migrationBuilder.DropTable(
                name: "GrowRooms");
        }
    }
}
